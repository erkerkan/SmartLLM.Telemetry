using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace SmartLLM.Telemetry.Providers.Ollama;

/// <summary>Minimal Ollama HTTP client implementing <see cref="IChatClient"/> via /api/chat.</summary>
internal sealed class OllamaHttpChatClient : IChatClient, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly Uri _endpoint;
    private readonly string _defaultModel;
    private readonly bool _ownsHttpClient;

    public OllamaHttpChatClient(Uri endpoint, string defaultModel, HttpClient? httpClient = null)
    {
        _endpoint = endpoint;
        _defaultModel = defaultModel;
        _ownsHttpClient = httpClient is null;
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
    }

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var request = BuildRequest(messages, options, stream: false);
        using var response = await _httpClient
            .PostAsJsonAsync(new Uri(_endpoint, "api/chat"), request, JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(JsonOptions, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new InvalidOperationException("Ollama returned an empty response body.");

        return ToChatResponse(payload);
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = BuildRequest(messages, options, stream: true);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(_endpoint, "api/chat"))
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };

        using var response = await _httpClient.SendAsync(
            httpRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var reader = new StreamReader(stream);

        while (true)
        {
            var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (line is null)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var chunk = JsonSerializer.Deserialize<OllamaChatResponse>(line, JsonOptions);
            if (chunk?.Message?.Content is { Length: > 0 } text)
            {
                yield return new ChatResponseUpdate(ChatRole.Assistant, text);
            }

            if (chunk?.Done == true)
            {
                break;
            }
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
        => serviceType == typeof(ChatClientMetadata)
            ? new ChatClientMetadata("ollama", new Uri(_endpoint, "/"))
            : null;

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    private OllamaChatRequest BuildRequest(IEnumerable<ChatMessage> messages, ChatOptions? options, bool stream)
        => new()
        {
            Model = options?.ModelId ?? _defaultModel,
            Messages = messages.Select(m => new OllamaMessage
            {
                Role = m.Role.Value,
                Content = m.Text ?? string.Empty
            }).ToList(),
            Stream = stream
        };

    private static ChatResponse ToChatResponse(OllamaChatResponse payload)
    {
        var text = payload.Message?.Content ?? string.Empty;
        UsageDetails? usage = null;
        if (payload.PromptEvalCount is not null || payload.EvalCount is not null)
        {
            usage = new UsageDetails
            {
                InputTokenCount = payload.PromptEvalCount,
                OutputTokenCount = payload.EvalCount
            };
        }

        return new ChatResponse(new ChatMessage(ChatRole.Assistant, text)) { Usage = usage };
    }

    private sealed class OllamaChatRequest
    {
        public string Model { get; set; } = string.Empty;

        public List<OllamaMessage> Messages { get; set; } = [];

        public bool Stream { get; set; }
    }

    private sealed class OllamaMessage
    {
        public string Role { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
    }

    private sealed class OllamaChatResponse
    {
        public OllamaMessage? Message { get; set; }

        public bool Done { get; set; }

        public int? PromptEvalCount { get; set; }

        public int? EvalCount { get; set; }
    }
}
