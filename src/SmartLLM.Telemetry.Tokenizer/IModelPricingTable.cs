namespace SmartLLM.Telemetry.Tokenizer;

/// <summary>Per-model USD pricing for offline cost estimation.</summary>
public interface IModelPricingTable
{
    double EstimateCostUsd(string model, int promptTokens, int completionTokens);
}

/// <summary>Default pricing table (USD per 1M tokens).</summary>
public sealed class ModelPricingTable : IModelPricingTable
{
    public static ModelPricingTable Default { get; } = new();

    private readonly Dictionary<string, (double InputPerMillion, double OutputPerMillion)> _prices =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["gpt-4o"] = (2.50, 10.00),
            ["gpt-4o-mini"] = (0.15, 0.60),
            ["gpt-4"] = (30.00, 60.00),
            ["gpt-3.5-turbo"] = (0.50, 1.50),
            ["text-embedding-3-small"] = (0.02, 0),
            ["text-embedding-3-large"] = (0.13, 0),
            ["text-embedding-ada-002"] = (0.10, 0),
            ["llama3"] = (0, 0),
            ["local-model"] = (0, 0),
            ["default"] = (1.00, 2.00)
        };

    private static readonly Dictionary<string, string> Aliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["gpt-35-turbo"] = "gpt-3.5-turbo",
            ["gpt-35-turbo-16k"] = "gpt-3.5-turbo"
        };

    public double EstimateCostUsd(string model, int promptTokens, int completionTokens)
    {
        var key = ResolvePricingKey(model);
        if (!_prices.TryGetValue(key, out var price))
        {
            price = _prices["default"];
        }

        return (promptTokens * price.InputPerMillion / 1_000_000d)
            + (completionTokens * price.OutputPerMillion / 1_000_000d);
    }

    public static string ResolvePricingKey(string model)
    {
        if (string.IsNullOrWhiteSpace(model))
        {
            return "default";
        }

        if (Aliases.TryGetValue(model, out var alias))
        {
            return alias;
        }

        var lower = model.ToLowerInvariant();
        if (lower.Contains("llama", StringComparison.Ordinal)
            || lower.StartsWith("local", StringComparison.Ordinal)
            || lower.Contains("ollama", StringComparison.Ordinal))
        {
            return "llama3";
        }

        if (lower.StartsWith("gpt-4o-mini", StringComparison.Ordinal))
        {
            return "gpt-4o-mini";
        }

        if (lower.StartsWith("gpt-4o", StringComparison.Ordinal))
        {
            return "gpt-4o";
        }

        if (lower.Contains("embedding", StringComparison.Ordinal))
        {
            return lower.Contains("large", StringComparison.Ordinal)
                ? "text-embedding-3-large"
                : "text-embedding-3-small";
        }

        return model;
    }
}
