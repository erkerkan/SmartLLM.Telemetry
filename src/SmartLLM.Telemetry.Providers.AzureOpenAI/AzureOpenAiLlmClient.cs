using System.Diagnostics;
using Microsoft.Extensions.Options;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.Extensions.AI;

namespace SmartLLM.Telemetry.Providers.AzureOpenAI;

/// <summary>Azure OpenAI LLM client backed by Azure.AI.OpenAI and Microsoft.Extensions.AI.</summary>
public sealed class AzureOpenAiLlmClient : ILlmClient
{
    private readonly AzureOpenAiChatClientHolder _chatClientHolder;
    private readonly Func<LlmRequest, CancellationToken, Task<LlmResponse>>? _testHandler;

    public AzureOpenAiLlmClient(
        AzureOpenAiChatClientHolder chatClientHolder,
        IOptions<AzureOpenAiProviderOptions> options,
        Func<LlmRequest, CancellationToken, Task<LlmResponse>>? testHandler = null)
    {
        _ = options;
        _chatClientHolder = chatClientHolder;
        _testHandler = testHandler;
    }

    public string Provider => "azure_openai";

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
