using SmartLLM.Telemetry.Core;
using Xunit;

namespace SmartLLM.Telemetry.Core.Tests;

public class LlmInterceptorPipelineTests
{
    [Fact]
    public async Task Pipeline_runs_interceptors_in_order()
    {
        var order = new List<string>();
        var inner = new StubClient();
        var pipeline = new LlmInterceptorPipeline(inner,
        [
            new RecordingInterceptor(order, "first", 1),
            new RecordingInterceptor(order, "second", 2)
        ]);

        await pipeline.CompleteAsync(new LlmRequest
        {
            Model = "gpt-4o-mini",
            Messages = [new LlmMessage { Role = "user", Content = "hi" }]
        });

        Assert.Equal(["first-pre", "second-pre", "second-post", "first-post"], order);
    }

    private sealed class StubClient : ILlmClient
    {
        public string Provider => "test";

        public Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new LlmResponse
            {
                Content = "ok",
                Model = request.Model,
                Duration = TimeSpan.FromMilliseconds(1)
            });
    }

    private sealed class RecordingInterceptor(List<string> order, string name, int orderValue) : ILlmInterceptor
    {
        public int Order => orderValue;

        public ValueTask OnExecutingAsync(LlmExecutionContext context, CancellationToken cancellationToken = default)
        {
            order.Add($"{name}-pre");
            return ValueTask.CompletedTask;
        }

        public ValueTask OnExecutedAsync(LlmExecutionContext context, CancellationToken cancellationToken = default)
        {
            order.Add($"{name}-post");
            return ValueTask.CompletedTask;
        }
    }
}
