namespace SmartLLM.Telemetry.Core;

/// <summary>Provider-agnostic embedding client abstraction.</summary>
public interface IEmbeddingClient
{
    string Provider { get; }

    Task<EmbeddingResponse> EmbedAsync(EmbeddingRequest request, CancellationToken cancellationToken = default);
}
