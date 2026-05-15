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
            ["llama3"] = (0, 0),
            ["default"] = (1.00, 2.00)
        };

    public double EstimateCostUsd(string model, int promptTokens, int completionTokens)
    {
        if (!_prices.TryGetValue(model, out var price))
        {
            price = _prices["default"];
        }

        return (promptTokens * price.InputPerMillion / 1_000_000d)
            + (completionTokens * price.OutputPerMillion / 1_000_000d);
    }
}
