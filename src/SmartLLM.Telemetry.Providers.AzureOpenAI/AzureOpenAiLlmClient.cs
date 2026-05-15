using System.Diagnostics;
using SmartLLM.Telemetry.Core;

namespace SmartLLM.Telemetry.Providers.AzureOpenAI;

/// <summary>Azure OpenAI adapter stub for Phase 1.</summary>
public sealed class AzureOpenAiLlmClient : ILlmClient
{
    public string Provider => "azure_openai";

    public async Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        await Task.Delay(10, cancellationToken).ConfigureAwait(false);
        stopwatch.Stop();

        return new LlmResponse
        {
            Content = $"[azure-openai-stub] model={request.Model}",
            Model = request.Model,
            FinishReason = "stop",
            Duration = stopwatch.Elapsed
        };
    }
}
