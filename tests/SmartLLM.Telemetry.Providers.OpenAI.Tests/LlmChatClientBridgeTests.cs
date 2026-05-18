using Microsoft.Extensions.AI;
using SmartLLM.Telemetry.Extensions.AI;
using Xunit;

namespace SmartLLM.Telemetry.Providers.OpenAI.Tests;

public sealed class ChatClientLlmBridgeTests
{
    [Fact]
    public void ToLlmResponse_maps_provider_usage()
    {
        var chatResponse = new ChatResponse(new ChatMessage(ChatRole.Assistant, "hi"))
        {
            Usage = new UsageDetails
            {
                InputTokenCount = 5,
                OutputTokenCount = 3
            }
        };

        var llmResponse = ChatClientLlmBridge.ToLlmResponse(chatResponse, "gpt-4o-mini", TimeSpan.FromMilliseconds(50));

        Assert.Equal("hi", llmResponse.Content);
        Assert.NotNull(llmResponse.Usage);
        Assert.Equal(5, llmResponse.Usage!.PromptTokens);
        Assert.Equal(3, llmResponse.Usage.CompletionTokens);
        Assert.False(llmResponse.Usage.IsEstimated);
    }
}
