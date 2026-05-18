using SmartLLM.Telemetry.Tokenizer;
using Xunit;

namespace SmartLLM.Telemetry.Tokenizer.Tests;

public class OfflineTokenCounterTests
{
    [Fact]
    public void CountTokens_uses_char_heuristic()
    {
        var counter = new OfflineTokenCounter();
        var tokens = counter.CountTokens("gpt-4o-mini", new string('a', 40));
        Assert.Equal(10, tokens);
    }

    [Fact]
    public void ResolvePricingKey_maps_local_llama_models_to_zero_cost_bucket()
    {
        Assert.Equal("llama3", ModelPricingTable.ResolvePricingKey("meta-llama-3-8b-instruct"));
        Assert.Equal(0, ModelPricingTable.Default.EstimateCostUsd("meta-llama-3-8b-instruct", 100, 100));
    }

    [Fact]
    public void EstimateUsage_calculates_cost()
    {
        var counter = new OfflineTokenCounter();
        var estimate = counter.EstimateUsage("gpt-4o-mini", ["hello world"], "response");
        Assert.True(estimate.PromptTokens > 0);
        Assert.True(estimate.CompletionTokens > 0);
        Assert.True(estimate.EstimatedCostUsd >= 0);
    }
}
