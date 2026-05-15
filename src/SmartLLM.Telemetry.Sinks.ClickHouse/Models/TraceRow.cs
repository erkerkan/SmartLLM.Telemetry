namespace SmartLLM.Telemetry.Sinks.ClickHouse;

/// <summary>Row model for traces table.</summary>
public sealed class TraceRow
{
    public DateTimeOffset EventTime { get; init; } = DateTimeOffset.UtcNow;

    public required string TraceId { get; init; }

    public required string SpanId { get; init; }

    public string ParentSpanId { get; init; } = string.Empty;

    public required string ServiceName { get; init; }

    public required string Operation { get; init; }

    public required string Provider { get; init; }

    public required string ModelName { get; init; }

    public required string Status { get; init; }

    public uint DurationMs { get; init; }

    public uint PromptTokens { get; init; }

    public uint CompletionTokens { get; init; }

    public uint TotalTokens { get; init; }

    public double EstimatedCostUsd { get; init; }

    public string TenantId { get; init; } = string.Empty;
}
