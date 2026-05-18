using System.Diagnostics;
using Microsoft.Extensions.Options;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.Extensions.AI;

namespace SmartLLM.Telemetry.Providers.OpenAI;

/// <summary>OpenAI LLM client backed by Microsoft.Extensions.AI.OpenAI.</summary>
public sealed class OpenAiLlmClient : ILlmClient
{
    private readonly OpenAiChatClientHolder _chatClientHolder;
    private readonly Func<LlmRequest, CancellationToken, Task<LlmResponse>>? _testHandler;

    public OpenAiLlmClient(
        OpenAiChatClientHolder chatClientHolder,
        IOptions<OpenAiProviderOptions> options,
        Func<LlmRequest, CancellationToken, Task<LlmResponse>>? testHandler = null)
    {
        _ = options;
        _chatClientHolder = chatClientHolder;
        _testHandler = testHandler;
    }

    public string Provider => "openai";

    public async Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        if (_testHandler is not null)
        {
            return await _testHandler(request, cancellationToken).ConfigureAwait(false);
        }

        var stopwatch = Stopwatch.StartNew();
        var messages = ChatClientLlmBridge.ToChatMessages(request.Messages);
        var chatOptions = ChatClientLlmBridge.ToChatOptions(request);

        var response = await _chatClientHolder.RawClient
            .GetResponseAsync(messages, chatOptions, cancellationToken)
            .ConfigureAwait(false);
        stopwatch.Stop();

        return ChatClientLlmBridge.ToLlmResponse(response, request.Model, stopwatch.Elapsed);
    }
}
