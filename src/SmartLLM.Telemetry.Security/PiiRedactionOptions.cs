using System.Text.RegularExpressions;

namespace SmartLLM.Telemetry.Security;

/// <summary>PII redaction configuration.</summary>
public sealed class PiiRedactionOptions
{
    public bool Enabled { get; set; } = true;

    public bool RedactInPrompts { get; set; } = true;

    public bool RedactInCompletions { get; set; } = true;

    public IList<(Regex Pattern, string Type)> CustomPatterns { get; } = new List<(Regex, string)>();
}
