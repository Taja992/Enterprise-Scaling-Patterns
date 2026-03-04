using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace HappyHeadlines.Observability;

public static class ObservabilityExtensions
{
    /// <summary>
    /// Replaces the default ASP.NET Core logger with Serilog (reads log levels from
    /// appsettings.json), wires Serilog to Seq, and registers OpenTelemetry tracing
    /// with OTLP export to Seq.
    ///
    /// Logging guidance:
    ///   Trace / Debug  — method entry/exit, EF Core queries (dev only, controlled by appsettings)
    ///   Information    — request received, entity saved/retrieved/deleted, service start/stop
    ///   Warning        — business rule violations, entity not found, circuit breaker state change
    ///   Error          — unhandled exceptions, DB failures, downstream service errors
    ///   Never log      — passwords, tokens, PII — handled automatically by SensitivePropertyScrubber
    /// </summary>
    public static WebApplicationBuilder AddHappyHeadlinesObservability(
        this WebApplicationBuilder builder,
        string serviceName
    )
    {
        // ── Serilog ────────────────────────────────────────────────────────────────
        builder.Host.UseSerilog(
            (context, services, config) =>
            {
                var seqUrl =
                    context.Configuration["Seq:ServerUrl"]
                    ?? Environment.GetEnvironmentVariable("SEQ_URL")
                    ?? "http://seq:5341";

                config
                    .ReadFrom.Configuration(context.Configuration) // honours MinimumLevel in appsettings
                    .ReadFrom.Services(services) // allows DI-registered sinks/enrichers
                    .Enrich.FromLogContext() // picks up LogContext.PushProperty calls
                    .Enrich.WithMachineName()
                    .Enrich.WithEnvironmentName()
                    .Enrich.WithThreadId()
                    .Enrich.WithProperty("ServiceName", serviceName)
                    .Enrich.WithProperty(
                        "ServiceVersion",
                        typeof(ObservabilityExtensions).Assembly.GetName().Version?.ToString()
                            ?? "unknown"
                    )
                    .Enrich.WithProperty(
                        "InstanceId",
                        context.Configuration["InstanceId"]
                            ?? Environment.GetEnvironmentVariable("INSTANCE_ID")
                            ?? "0"
                    )
                    .Enrich.With<SensitivePropertyScrubber>()
                    .WriteTo.Console()
                    .WriteTo.Seq(seqUrl);
            }
        );

        // ── OpenTelemetry ──────────────────────────────────────────────────────────
        var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://seq:4317";

        builder
            .Services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(serviceName).AddEnvironmentVariableDetector())
            .WithTracing(tracing =>
                tracing
                    .AddAspNetCoreInstrumentation(o =>
                    {
                        o.RecordException = true; // attaches exception details to the failing span
                    })
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation(o =>
                    {
                        // Do NOT set SetDbStatementForText = true in production —
                        // SQL may contain user-supplied values
                        o.SetDbStatementForText = false;
                    })
                    .AddOtlpExporter(o =>
                    {
                        o.Endpoint = new Uri(otlpEndpoint);
                        o.Protocol = OtlpExportProtocol.Grpc;
                    })
            );

        return builder;
    }

    /// <summary>
    /// Registers the CorrelationIdMiddleware and Serilog request logging.
    /// Call this BEFORE UseRouting and UseAuthorization in Program.cs.
    /// </summary>
    public static IApplicationBuilder UseHappyHeadlinesObservability(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();

        app.UseSerilogRequestLogging(o =>
        {
            // Single structured log line per request — replaces the default verbose ASP.NET lines
            o.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

            // Attach CorrelationId to the request completion event
            o.EnrichDiagnosticContext = (diag, ctx) =>
            {
                if (ctx.Items.TryGetValue("CorrelationId", out var id))
                    diag.Set("CorrelationId", id);
            };
        });

        return app;
    }
}
