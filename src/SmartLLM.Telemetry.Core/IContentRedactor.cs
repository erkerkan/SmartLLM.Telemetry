namespace SmartLLM.Telemetry.Core;

/// <summary>Redacts sensitive content before persistence or export.</summary>
public interface IContentRedactor
{
    string Redact(string input);
}

/// <summary>No-op redactor when security package is not registered.</summary>
public sealed class PassthroughContentRedactor : IContentRedactor
{
    public static PassthroughContentRedactor Instance { get; } = new();

    public string Redact(string input) => input;
}
