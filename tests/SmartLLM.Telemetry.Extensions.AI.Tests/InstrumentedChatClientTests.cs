using System.Diagnostics;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.Extensions.AI;
using SmartLLM.Telemetry.OpenTelemetry;
using SmartLLM.Telemetry.Tokenizer;
using Xunit;

namespace SmartLLM.Telemetry.Extensions.AI.Tests;

public class InstrumentedChatClientTests
{
    [Fact]
    public async Task GetResponseAsync_records_activity_tags()
    {
        Activity? captured = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = s => s.Name == SmartLLMTelemetryActivitySource.Name,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStopped = a => captured = a
        };
        ActivitySource.AddActivityListener(listener);

        var client = new InstrumentedChatClient(
            new StubChatClient(),
            Options.Create(new SmartLLMTelemetryOptions()),
            new TiktokenTokenCounter());

        var response = await client.GetResponseAsync(
        [
            new ChatMessage(ChatRole.User, "Hello")
        ],
            new ChatOptions { ModelId = "gpt-4o-mini" });

        Assert.NotNull(response.Text);
        Assert.NotNull(captured);
        Assert.Equal("gpt-4o-mini", captured!.GetTagItem(SmartLLMTelemetryActivitySource.Tags.ModelName));
        Assert.Equal("ok", captured.GetTagItem(SmartLLMTelemetryActivitySource.Tags.Status));
    }

    private sealed class StubChatClient : IChatClient
    {
        public async Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return new ChatResponse(new ChatMessage(ChatRole.Assistant, "Hi there"))
            {
                Usage = new UsageDetails
                {
                    InputTokenCount = 5,
                    OutputTokenCount = 3
                }
            };
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose() { }
    }
}
