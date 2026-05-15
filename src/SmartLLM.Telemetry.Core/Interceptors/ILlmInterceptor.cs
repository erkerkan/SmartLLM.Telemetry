namespace SmartLLM.Telemetry.Core;

/// <summary>Pipeline interceptor for pre/post LLM call processing.</summary>
public interface ILlmInterceptor
{
    /// <summary>Order (lower runs first). Default 0.</summary>
    int Order => 0;

    ValueTask OnExecutingAsync(LlmExecutionContext context, CancellationToken cancellationToken = default);

    ValueTask OnExecutedAsync(LlmExecutionContext context, CancellationToken cancellationToken = default);
}

/// <summary>Mutable context passed through the interceptor pipeline.</summary>
public sealed class LlmExecutionContext
{
    public required LlmRequest Request { get; init; }

    public LlmResponse? Response { get; set; }

    public Exception? Exception { get; set; }

    public LlmUsage? Usage { get; set; }

    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>();
}
