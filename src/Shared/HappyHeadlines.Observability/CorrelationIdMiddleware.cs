using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace HappyHeadlines.Observability;

/// <summary>
/// Reads X-Correlation-ID from the inbound request (or generates a new GUID),
/// echoes it on the response, pushes it onto the Serilog log context, and adds it
/// as OpenTelemetry baggage so it flows to every downstream service and span.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId =
            context.Request.Headers[HeaderName].FirstOrDefault() ?? Guid.NewGuid().ToString();

        // Echo back so the caller can track its own request
        context.Response.Headers[HeaderName] = correlationId;

        // Available to endpoint code via context.Items
        context.Items["CorrelationId"] = correlationId;

        // Attach to the current OTel span so it appears on every child span
        System.Diagnostics.Activity.Current?.SetBaggage("correlation.id", correlationId);

        // Push onto Serilog ambient context — every log statement inside _next carries it
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
