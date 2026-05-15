using System.Diagnostics;
using SmartLLM.Telemetry.Core;

namespace SmartLLM.Telemetry.Providers.Ollama;

/// <summary>Ollama adapter stub for Phase 1.</summary>
public sealed class OllamaLlmClient : ILlmClient
{
    public string Provider => "ollama";

    public async Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        await Task.Delay(10, cancellationToken).ConfigureAwait(false);
        stopwatch.Stop();

        return new LlmResponse
        {
            Content = $"[ollama-stub] model={request.Model}",
            Model = request.Model,
            FinishReason = "stop",
            Duration = stopwatch.Elapsed
        };
    }
}
