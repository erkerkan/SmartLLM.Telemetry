namespace SmartLLM.Telemetry.Sinks.ClickHouse;

/// <summary>Row model for costs table.</summary>
public sealed class CostRow
{
    public DateTimeOffset EventTime { get; init; } = DateTimeOffset.UtcNow;

    public string TenantId { get; init; } = string.Empty;

    public string ApiKeyHash { get; init; } = string.Empty;

    public required string Provider { get; init; }

    public required string ModelName { get; init; }

    public uint PromptTokens { get; init; }

    public uint CompletionTokens { get; init; }

    public uint TotalTokens { get; init; }

    public double CostUsd { get; init; }

    public string Currency { get; init; } = "USD";
}
