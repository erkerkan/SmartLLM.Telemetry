namespace SmartLLM.Telemetry.Tokenizer;

/// <summary>Provider-independent token estimation.</summary>
public interface ITokenCounter
{
    int CountTokens(string model, string text);

    LlmUsageEstimate EstimateUsage(string model, IReadOnlyList<string> promptParts, string? completion = null);
}

/// <summary>Estimated usage without provider response.</summary>
public sealed class LlmUsageEstimate
{
    public int PromptTokens { get; init; }

    public int CompletionTokens { get; init; }

    public int TotalTokens => PromptTokens + CompletionTokens;

    public double EstimatedCostUsd { get; init; }
}
