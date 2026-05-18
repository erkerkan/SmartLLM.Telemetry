using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.Extensions.AI;
using SmartLLM.Telemetry.OpenTelemetry;
using SmartLLM.Telemetry.Tokenizer;
using Xunit;

namespace SmartLLM.Telemetry.Extensions.AI.Tests;

public sealed class InstrumentedChatClientStreamingTests
{
    [Fact]
    public async Task GetStreamingResponseAsync_sets_token_tags_after_stream_completes()
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
            new StreamingStubChatClient(),
            Options.Create(new SmartLLMTelemetryOptions()),
            new TiktokenTokenCounter());

        var parts = new List<string>();
        await foreach (var update in client.GetStreamingResponseAsync(
                           [new ChatMessage(ChatRole.User, "Hello")],
                           new ChatOptions { ModelId = "gpt-4o-mini" }))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                parts.Add(update.Text);
            }
        }

        Assert.Equal("Hi", string.Concat(parts));
        Assert.NotNull(captured);
        Assert.Equal("chat_stream", captured!.GetTagItem(SmartLLMTelemetryActivitySource.Tags.Operation));
        Assert.Equal("ok", captured.GetTagItem(SmartLLMTelemetryActivitySource.Tags.Status));
        Assert.NotNull(captured.GetTagItem(SmartLLMTelemetryActivitySource.Tags.TotalTokens));
    }

    private sealed class StreamingStubChatClient : IChatClient
    {
        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            yield return new ChatResponseUpdate(ChatRole.Assistant, "Hi");
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose()
        {
        }
    }
}
