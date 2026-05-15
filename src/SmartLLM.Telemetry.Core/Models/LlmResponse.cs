namespace SmartLLM.Telemetry.Core;

/// <summary>LLM chat completion response.</summary>
public sealed class LlmResponse
{
    public required string Content { get; init; }

    public required string Model { get; init; }

    public LlmUsage? Usage { get; init; }

    public string? FinishReason { get; init; }

    public TimeSpan Duration { get; init; }
}

/// <summary>Token usage reported by provider or estimated offline.</summary>
public sealed class LlmUsage
{
    public int PromptTokens { get; init; }

    public int CompletionTokens { get; init; }

    public int TotalTokens => PromptTokens + CompletionTokens;

    public double? EstimatedCostUsd { get; init; }

    public bool IsEstimated { get; init; }
}
