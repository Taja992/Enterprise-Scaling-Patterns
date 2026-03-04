using Serilog.Core;
using Serilog.Events;

namespace HappyHeadlines.Observability;

/// <summary>
/// Serilog enricher that redacts known sensitive property names before log events
/// leave the process. Add new sensitive names here — they will be scrubbed
/// across every service automatically.
/// </summary>
public sealed class SensitivePropertyScrubber : ILogEventEnricher
{
    private static readonly HashSet<string> SensitiveNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password",
        "Token",
        "Authorization",
        "Secret",
        "ApiKey",
        "CardNumber",
        "Ssn",
    };

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        foreach (var name in SensitiveNames)
        {
            if (logEvent.Properties.ContainsKey(name))
            {
                logEvent.AddOrUpdateProperty(
                    propertyFactory.CreateProperty(name, "***REDACTED***")
                );
            }
        }
    }
}
