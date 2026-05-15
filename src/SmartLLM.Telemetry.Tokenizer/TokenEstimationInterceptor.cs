using SmartLLM.Telemetry.Core;

namespace SmartLLM.Telemetry.Tokenizer;

/// <summary>Fills usage estimates when provider does not return token counts.</summary>
public sealed class TokenEstimationInterceptor : ILlmInterceptor
{
    public const string UsageItemKey = "smartllm.usage.estimate";

    private readonly ITokenCounter _counter;

    public TokenEstimationInterceptor(ITokenCounter counter)
    {
        _counter = counter;
    }

    public int Order => 100;

    public ValueTask OnExecutingAsync(LlmExecutionContext context, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    public ValueTask OnExecutedAsync(LlmExecutionContext context, CancellationToken cancellationToken = default)
    {
        if (context.Response is null)
        {
            return ValueTask.CompletedTask;
        }

        if (context.Response.Usage is { IsEstimated: false })
        {
            return ValueTask.CompletedTask;
        }

        var promptParts = context.Request.Messages.Select(m => m.Content).ToArray();
        var estimate = _counter.EstimateUsage(
            context.Request.Model,
            promptParts,
            context.Response.Content);

        context.Usage = new LlmUsage
        {
            PromptTokens = estimate.PromptTokens,
            CompletionTokens = estimate.CompletionTokens,
            EstimatedCostUsd = estimate.EstimatedCostUsd,
            IsEstimated = true
        };

        context.Items[UsageItemKey] = estimate;
        return ValueTask.CompletedTask;
    }
}
