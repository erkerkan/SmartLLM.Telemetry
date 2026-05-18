namespace SmartLLM.Telemetry.Core;

/// <summary>Embedding generation response.</summary>
public sealed class EmbeddingResponse
{
    public IReadOnlyList<ReadOnlyMemory<float>> Vectors { get; init; } = [];

    public int Dimensions { get; init; }

    public LlmUsage? Usage { get; init; }
}
