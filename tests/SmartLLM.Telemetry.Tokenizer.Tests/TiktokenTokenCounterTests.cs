using SmartLLM.Telemetry.Tokenizer;
using Xunit;

namespace SmartLLM.Telemetry.Tokenizer.Tests;

public class TiktokenTokenCounterTests
{
    [Fact]
    public void CountTokens_uses_tiktoken_for_gpt4o_mini()
    {
        var text = new string('a', 200);
        var counter = new TiktokenTokenCounter();
        var tiktokenCount = counter.CountTokens("gpt-4o-mini", text);
        var heuristicCount = new OfflineTokenCounter().CountTokens("gpt-4o-mini", text);
        Assert.True(tiktokenCount > 0);
        Assert.Equal(50, heuristicCount);
        Assert.NotEqual(heuristicCount, tiktokenCount);
    }

    [Fact]
    public void CountTokens_falls_back_for_unknown_model()
    {
        var counter = new TiktokenTokenCounter();
        var tokens = counter.CountTokens("unknown-model-xyz", new string('a', 40));
        Assert.Equal(10, tokens);
    }
}
