namespace SmartLLM.Telemetry.Core;

/// <summary>Embedding generation request.</summary>
public sealed class EmbeddingRequest
{
    public string Model { get; init; } = "text-embedding-3-small";

    public IReadOnlyList<string> Inputs { get; init; } = [];

    public string? TenantId { get; init; }

    public string? RequestId { get; init; }
}
