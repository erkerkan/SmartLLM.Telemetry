using SmartLLM.Telemetry.Core;

namespace SmartLLM.Telemetry.Security;

/// <summary>Adapts <see cref="PiiRedactor"/> for export paths (ClickHouse, etc.).</summary>
public sealed class PiiContentRedactor : IContentRedactor
{
    private readonly PiiRedactor _redactor;

    public PiiContentRedactor(PiiRedactor redactor) => _redactor = redactor;

    public string Redact(string input) => _redactor.Redact(input);
}
