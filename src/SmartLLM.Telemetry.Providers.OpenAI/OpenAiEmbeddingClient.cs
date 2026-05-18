using System.ClientModel;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using OpenAI.Embeddings;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.Tokenizer;

namespace SmartLLM.Telemetry.Providers.OpenAI;

/// <summary>OpenAI embeddings via the official OpenAI .NET SDK.</summary>
public sealed class OpenAiEmbeddingClient : IEmbeddingClient
{
    private readonly EmbeddingClient _client;
    private readonly ITokenCounter _tokenCounter;
    private readonly string _defaultModel;
    private readonly Func<EmbeddingRequest, CancellationToken, Task<EmbeddingResponse>>? _testHandler;

    public OpenAiEmbeddingClient(
        IOptions<OpenAiProviderOptions> options,
        ITokenCounter tokenCounter,
        Func<EmbeddingRequest, CancellationToken, Task<EmbeddingResponse>>? testHandler = null)
    {
        _tokenCounter = tokenCounter;
        _testHandler = testHandler;
        var o = options.Value;
        _defaultModel = o.EmbeddingModel;

        var apiKey = o.ApiKey ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _client = null!;
            return;
        }

        _ = o.Endpoint;
        _client = new EmbeddingClient(_defaultModel, new ApiKeyCredential(apiKey));
    }

    public string Provider => "openai";

    public async Task<EmbeddingResponse> EmbedAsync(EmbeddingRequest request, CancellationToken cancellationToken = default)
    {
        if (_testHandler is not null)
        {
            return await _testHandler(request, cancellationToken).ConfigureAwait(false);
        }

        if (_client is null)
        {
            return CreateStubResponse(request);
        }

        var model = string.IsNullOrWhiteSpace(request.Model) ? _defaultModel : request.Model;
        var vectors = new List<ReadOnlyMemory<float>>();

        foreach (var input in request.Inputs)
        {
            var embedding = await _client.GenerateEmbeddingAsync(input, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            vectors.Add(embedding.Value.ToFloats().ToArray());
        }

        var dimensions = vectors.Count > 0 ? vectors[0].Length : 0;
        var estimate = _tokenCounter.EstimateUsage(model, request.Inputs.ToArray(), completion: null);

        return new EmbeddingResponse
        {
            Vectors = vectors,
            Dimensions = dimensions,
            Usage = new LlmUsage
            {
                PromptTokens = estimate.PromptTokens,
                CompletionTokens = 0,
                EstimatedCostUsd = estimate.EstimatedCostUsd,
                IsEstimated = true
            }
        };
    }

    private static EmbeddingResponse CreateStubResponse(EmbeddingRequest request)
    {
        const int dimensions = 8;
        var vectors = request.Inputs
            .Select(_ => (ReadOnlyMemory<float>)new float[dimensions])
            .ToList();

        return new EmbeddingResponse
        {
            Vectors = vectors,
            Dimensions = dimensions,
            Usage = new LlmUsage { PromptTokens = 1, IsEstimated = true }
        };
    }
}
