using System.Runtime.CompilerServices;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
namespace SmartLLM.Telemetry.Providers.AzureOpenAI;

/// <summary>Creates the underlying Azure OpenAI <see cref="IChatClient"/> (or stub).</summary>
internal static class AzureOpenAiChatClientFactory
{
    public static IChatClient Create(IOptions<AzureOpenAiProviderOptions> options)
    {
        var o = options.Value;
        var endpoint = o.Endpoint?.ToString()
            ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var apiKey = o.ApiKey ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        var deployment = o.DeploymentName
            ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT")
            ?? "gpt-4o-mini";

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
        {
            if (!o.UseStubWhenNoCredentials)
            {
                throw new InvalidOperationException(
                    "Azure OpenAI endpoint and API key are required. Set SmartLLM:AzureOpenAI or AZURE_OPENAI_* environment variables.");
            }

            return new StubChatClient(deployment);
        }

        var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        return client.GetChatClient(deployment).AsIChatClient();
    }

    private sealed class StubChatClient : IChatClient
    {
        private readonly string _deployment;

        public StubChatClient(string deployment) => _deployment = deployment;

        public async Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(10, cancellationToken).ConfigureAwait(false);
            var lastUser = messages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? "(empty)";
            return new ChatResponse(new ChatMessage(ChatRole.Assistant, $"[azure-openai-stub:{_deployment}] echo: {lastUser}"));
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
