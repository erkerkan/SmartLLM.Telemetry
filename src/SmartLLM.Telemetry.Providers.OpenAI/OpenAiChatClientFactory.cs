using System.ClientModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;

namespace SmartLLM.Telemetry.Providers.OpenAI;

/// <summary>Creates the underlying OpenAI <see cref="IChatClient"/> (or stub).</summary>
internal static class OpenAiChatClientFactory
{
    public static IChatClient Create(IOptions<OpenAiProviderOptions> options)
    {
        var o = options.Value;
        var apiKey = o.ApiKey ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            if (!o.UseStubWhenNoApiKey)
            {
                throw new InvalidOperationException(
                    "OpenAI API key is required. Set SmartLLM:OpenAI:ApiKey or OPENAI_API_KEY.");
            }

            return new StubChatClient(o.Model);
        }

        OpenAIClient openAiClient = o.Endpoint is null
            ? new OpenAIClient(new ApiKeyCredential(apiKey))
            : new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions { Endpoint = o.Endpoint });

        return openAiClient.GetChatClient(o.Model).AsIChatClient();
    }

    private sealed class StubChatClient : IChatClient
    {
        private readonly string _model;

        public StubChatClient(string model) => _model = model;

        public async Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(10, cancellationToken).ConfigureAwait(false);
            var lastUser = messages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? "(empty)";
            return new ChatResponse(new ChatMessage(ChatRole.Assistant, $"[openai-stub:{_model}] echo: {lastUser}"));
        }

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var response = await GetResponseAsync(messages, options, cancellationToken).ConfigureAwait(false);
            yield return new ChatResponseUpdate(ChatRole.Assistant, response.Text);
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose()
        {
        }
    }
}
