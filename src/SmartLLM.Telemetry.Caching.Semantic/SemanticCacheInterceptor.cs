using SmartLLM.Telemetry.Core;

namespace SmartLLM.Telemetry.Caching.Semantic;

/// <summary>Phase 2 placeholder for vector similarity cache lookup.</summary>
public sealed class SemanticCacheInterceptor : ILlmInterceptor
{
    public int Order => 50;

    public ValueTask OnExecutingAsync(LlmExecutionContext context, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    public ValueTask OnExecutedAsync(LlmExecutionContext context, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;
}
