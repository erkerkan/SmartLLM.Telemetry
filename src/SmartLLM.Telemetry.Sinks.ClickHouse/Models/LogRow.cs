namespace SmartLLM.Telemetry.Sinks.ClickHouse;

/// <summary>Row model for logs table.</summary>
public sealed class LogRow
{
    public DateTimeOffset EventTime { get; init; } = DateTimeOffset.UtcNow;

    public required string TraceId { get; init; }

    public required string Severity { get; init; }

    public required string Message { get; init; }

    public IReadOnlyDictionary<string, string> Attributes { get; init; } = new Dictionary<string, string>();
}
