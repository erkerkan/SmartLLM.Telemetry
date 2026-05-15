namespace SmartLLM.Telemetry.Tokenizer;

/// <summary>
/// Heuristic offline token counter (chars/4 approximation).
/// Replace with tiktoken-backed implementation in a future release.
/// </summary>
public sealed class OfflineTokenCounter : ITokenCounter
{
    private readonly IModelPricingTable _pricing;

    public OfflineTokenCounter(IModelPricingTable? pricing = null)
    {
        _pricing = pricing ?? ModelPricingTable.Default;
    }

    public int CountTokens(string model, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        // GPT-style heuristic: ~4 characters per token for English text.
        return (int)Math.Ceiling(text.Length / 4.0);
    }

    public LlmUsageEstimate EstimateUsage(string model, IReadOnlyList<string> promptParts, string? completion = null)
    {
        var promptTokens = promptParts.Sum(p => CountTokens(model, p));
        var completionTokens = completion is null ? 0 : CountTokens(model, completion);
        var cost = _pricing.EstimateCostUsd(model, promptTokens, completionTokens);

        return new LlmUsageEstimate
        {
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
            EstimatedCostUsd = cost
        };
    }
}
