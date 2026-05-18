using System.Diagnostics;
using SmartLLM.Telemetry.OpenTelemetry;

namespace SmartLLM.Telemetry.Sinks.ClickHouse;

/// <summary>Collects non-standard activity tags for ClickHouse Map columns.</summary>
internal static class ActivityTagCollector
{
    private static readonly HashSet<string> KnownTags = new(StringComparer.Ordinal)
    {
        SmartLLMTelemetryActivitySource.Tags.Provider,
        SmartLLMTelemetryActivitySource.Tags.ModelName,
        SmartLLMTelemetryActivitySource.Tags.Operation,
        SmartLLMTelemetryActivitySource.Tags.RequestId,
        SmartLLMTelemetryActivitySource.Tags.Status,
        SmartLLMTelemetryActivitySource.Tags.PromptTokens,
        SmartLLMTelemetryActivitySource.Tags.CompletionTokens,
        SmartLLMTelemetryActivitySource.Tags.TotalTokens,
        SmartLLMTelemetryActivitySource.Tags.EstimatedCostUsd,
        SmartLLMTelemetryActivitySource.Tags.DurationMs,
        SmartLLMTelemetryActivitySource.Tags.TenantId,
        SmartLLMTelemetryActivitySource.Tags.ApiKeyId
    };

    public static Dictionary<string, string> Collect(Activity activity)
    {
        var attributes = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var tag in activity.TagObjects)
        {
            if (tag.Key is null || KnownTags.Contains(tag.Key))
            {
                continue;
            }

            var value = FormatTagValue(tag.Value);
            if (value is not null)
            {
                attributes[tag.Key] = value;
            }
        }

        return attributes;
    }

    private static string? FormatTagValue(object? value)
        => value switch
        {
            null => null,
            string s => s,
            bool b => b ? "true" : "false",
            IFormattable f => f.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
}
