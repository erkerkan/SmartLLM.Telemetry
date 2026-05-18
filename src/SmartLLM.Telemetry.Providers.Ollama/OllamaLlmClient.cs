using System.Diagnostics;
using Microsoft.Extensions.Options;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.Extensions.AI;

namespace SmartLLM.Telemetry.Providers.Ollama;

/// <summary>Ollama LLM client backed by the Ollama HTTP API.</summary>
public sealed class OllamaLlmClient : ILlmClient
{
    private readonly OllamaChatClientHolder _chatClientHolder;
    private readonly OllamaProviderOptions _options;
    private readonly Func<LlmRequest, CancellationToken, Task<LlmResponse>>? _testHandler;

    public OllamaLlmClient(
        OllamaChatClientHolder chatClientHolder,
        IOptions<OllamaProviderOptions> options,
        Func<LlmRequest, CancellationToken, Task<LlmResponse>>? testHandler = null)
    {
        _chatClientHolder = chatClientHolder;
        _options = options.Value;
        _testHandler = testHandler;
    }

    public string Provider => "ollama";

    public async Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        if (_testHandler is not null)
        {
            return await _testHandler(request, cancellationToken).ConfigureAwait(false);
        }

        var stopwatch = Stopwatch.StartNew();
        var messages = ChatClientLlmBridge.ToChatMessages(request.Messages);
        var chatOptions = ChatClientLlmBridge.ToChatOptions(request);
        chatOptions.ModelId ??= _options.Model;

        var response = await _chatClientHolder.RawClient
            .GetResponseAsync(messages, chatOptions, cancellationToken)
            .ConfigureAwait(false);
        stopwatch.Stop();

        return ChatClientLlmBridge.ToLlmResponse(response, request.Model, stopwatch.Elapsed);
    }
}
