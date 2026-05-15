using System.Diagnostics;
using SmartLLM.Telemetry.Core;

namespace SmartLLM.Telemetry.Providers.OpenAI;

/// <summary>
/// OpenAI-compatible LLM client adapter.
/// Phase 1 ships a testable stub; wire to Microsoft.Extensions.AI OpenAI client in a follow-up.
/// </summary>
public class OpenAiLlmClient : ILlmClient
{
    private readonly Func<LlmRequest, CancellationToken, Task<LlmResponse>>? _handler;

    public OpenAiLlmClient(Func<LlmRequest, CancellationToken, Task<LlmResponse>>? handler = null)
    {
        _handler = handler;
    }

    public string Provider => "openai";

    public async Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        if (_handler is not null)
        {
            return await _handler(request, cancellationToken).ConfigureAwait(false);
        }

        await Task.Delay(10, cancellationToken).ConfigureAwait(false);
        stopwatch.Stop();

        var content = $"[openai-stub] model={request.Model}";
        return new LlmResponse
        {
            Content = content,
            Model = request.Model,
            FinishReason = "stop",
            Duration = stopwatch.Elapsed,
            Usage = null // offline tokenizer fills usage in pipeline
        };
    }
}
