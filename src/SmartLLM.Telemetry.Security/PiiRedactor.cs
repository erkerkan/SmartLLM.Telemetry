using System.Text.RegularExpressions;

namespace SmartLLM.Telemetry.Security;

/// <summary>Regex-based PII redactor.</summary>
public sealed class PiiRedactor
{
    private static readonly (Regex Pattern, string Type)[] DefaultPatterns =
    [
        (new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b", RegexOptions.Compiled), "email"),
        (new Regex(@"\b(?:\d[ -]*?){13,16}\b", RegexOptions.Compiled), "credit_card"),
        (new Regex(@"\b\d{3}-\d{2}-\d{4}\b", RegexOptions.Compiled), "ssn")
    ];

    private readonly PiiRedactionOptions _options;

    public PiiRedactor(Microsoft.Extensions.Options.IOptions<PiiRedactionOptions> options)
    {
        _options = options.Value;
    }

    public string Redact(string input)
    {
        if (!_options.Enabled || string.IsNullOrEmpty(input))
        {
            return input;
        }

        var result = input;
        foreach (var (pattern, type) in EnumeratePatterns())
        {
            result = pattern.Replace(result, $"[REDACTED:{type}]");
        }

        return result;
    }

    private IEnumerable<(Regex Pattern, string Type)> EnumeratePatterns()
    {
        foreach (var p in DefaultPatterns)
        {
            yield return p;
        }

        foreach (var p in _options.CustomPatterns)
        {
            yield return p;
        }
    }
}
