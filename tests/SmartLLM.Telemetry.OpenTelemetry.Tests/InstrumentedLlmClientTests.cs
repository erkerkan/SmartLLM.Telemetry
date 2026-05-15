using System.Diagnostics;
using Xunit;
using Microsoft.Extensions.Options;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.OpenTelemetry;

namespace SmartLLM.Telemetry.OpenTelemetry.Tests;

public class InstrumentedLlmClientTests
{
    [Fact]
    public async Task CompleteAsync_sets_token_tags_on_success()
    {
        Activity? captured = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = s => s.Name == SmartLLMTelemetryActivitySource.Name,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStopped = a => captured = a
        };
        ActivitySource.AddActivityListener(listener);

        var client = new InstrumentedLlmClient(
            new StubClient(),
            Options.Create(new SmartLLMTelemetryOptions()));

        await client.CompleteAsync(new LlmRequest
        {
            Model = "gpt-4o-mini",
            Messages = [new LlmMessage { Role = "user", Content = "hi" }]
        });

        Assert.NotNull(captured);
        Assert.Equal("ok", captured!.GetTagItem(SmartLLMTelemetryActivitySource.Tags.Status));
        Assert.Equal("gpt-4o-mini", captured.GetTagItem(SmartLLMTelemetryActivitySource.Tags.ModelName));
        Assert.Equal(15, captured.GetTagItem(SmartLLMTelemetryActivitySource.Tags.TotalTokens));
    }

    private sealed class StubClient : ILlmClient
    {
        public string Provider => "test";

        public Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new LlmResponse
            {
                Content = "ok",
                Model = request.Model,
                Duration = TimeSpan.FromMilliseconds(5),
                Usage = new LlmUsage { PromptTokens = 10, CompletionTokens = 5, IsEstimated = false }
            });
    }
}
