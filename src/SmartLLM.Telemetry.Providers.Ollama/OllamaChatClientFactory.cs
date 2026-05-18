using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace SmartLLM.Telemetry.Providers.Ollama;

/// <summary>Creates the underlying Ollama <see cref="IChatClient"/> (or stub).</summary>
internal static class OllamaChatClientFactory
{
    public static IChatClient Create(IOptions<OllamaProviderOptions> options)
    {
        var o = options.Value;
        var endpoint = ResolveEndpoint(o);
        var model = string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OLLAMA_MODEL"))
            ? o.Model
            : Environment.GetEnvironmentVariable("OLLAMA_MODEL")!;

        if (o.UseStubWhenUnavailable && !IsOllamaReachable(endpoint))
        {
            return new StubChatClient(model);
        }

        return new OllamaHttpChatClient(endpoint, model);
    }

    private static Uri ResolveEndpoint(OllamaProviderOptions options)
    {
        var host = Environment.GetEnvironmentVariable("OLLAMA_HOST");
        if (!string.IsNullOrWhiteSpace(host))
        {
            return new Uri(host.TrimEnd('/') + "/");
        }

        return options.Endpoint;
    }

    private static bool IsOllamaReachable(Uri endpoint)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var response = http.GetAsync(new Uri(endpoint, "api/tags")).GetAwaiter().GetResult();
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
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
            return new ChatResponse(new ChatMessage(ChatRole.Assistant, $"[ollama-stub:{_model}] echo: {lastUser}"));
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
