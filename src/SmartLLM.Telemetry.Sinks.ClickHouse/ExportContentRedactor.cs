using SmartLLM.Telemetry.Core;

namespace SmartLLM.Telemetry.Sinks.ClickHouse;

internal static class ExportContentRedactor
{
    public static string Apply(string value, IContentRedactor? redactor)
        => redactor is null || string.IsNullOrEmpty(value) ? value : redactor.Redact(value);

    public static Dictionary<string, string> ApplyAttributes(
        Dictionary<string, string> attributes,
        IContentRedactor? redactor)
    {
        if (redactor is null || attributes.Count == 0)
        {
            return attributes;
        }

        var redacted = new Dictionary<string, string>(attributes.Count, StringComparer.Ordinal);
        foreach (var (key, value) in attributes)
        {
            redacted[key] = redactor.Redact(value);
        }

        return redacted;
    }
}
