using SharpToken;

namespace SmartLLM.Telemetry.Tokenizer;

/// <summary>Tiktoken-backed token counter with heuristic fallback for unknown models.</summary>
public sealed class TiktokenTokenCounter : ITokenCounter
{
    private readonly IModelPricingTable _pricing;
    private readonly OfflineTokenCounter _fallback = new();

    public TiktokenTokenCounter(IModelPricingTable? pricing = null)
    {
        _pricing = pricing ?? ModelPricingTable.Default;
        _fallback = new OfflineTokenCounter(_pricing);
    }

    public int CountTokens(string model, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        try
        {
            var encoding = GptEncoding.GetEncodingForModel(NormalizeModel(model));
            return encoding.CountTokens(text);
        }
        catch (Exception)
        {
            return _fallback.CountTokens(model, text);
        }
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

    private static string NormalizeModel(string model)
    {
        // Azure deployment names and dated snapshots map to base families.
        if (model.StartsWith("gpt-4o-mini", StringComparison.OrdinalIgnoreCase))
        {
            return "gpt-4o-mini";
        }

        if (model.StartsWith("gpt-4o", StringComparison.OrdinalIgnoreCase))
        {
            return "gpt-4o";
        }

        if (model.StartsWith("gpt-4", StringComparison.OrdinalIgnoreCase))
        {
            return "gpt-4";
        }

        if (model.StartsWith("gpt-3.5", StringComparison.OrdinalIgnoreCase))
        {
            return "gpt-3.5-turbo";
        }

        return model;
    }
}
