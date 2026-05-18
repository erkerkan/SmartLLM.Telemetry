using System.Diagnostics;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.OpenTelemetry;
using SmartLLM.Telemetry.Tokenizer;

namespace SmartLLM.Telemetry.Extensions.AI;

/// <summary>Decorates <see cref="IEmbeddingGenerator{TInput, TEmbedding}"/> with SmartLLM telemetry.</summary>
public sealed class InstrumentedEmbeddingGenerator<TInput, TEmbedding> : IEmbeddingGenerator<TInput, TEmbedding>
    where TEmbedding : Embedding
{
    private readonly IEmbeddingGenerator<TInput, TEmbedding> _inner;
    private readonly SmartLLMTelemetryOptions _options;
    private readonly ITokenCounter _tokenCounter;

    public InstrumentedEmbeddingGenerator(
        IEmbeddingGenerator<TInput, TEmbedding> inner,
        IOptions<SmartLLMTelemetryOptions> telemetryOptions,
        ITokenCounter tokenCounter)
    {
        _inner = inner;
        _options = telemetryOptions.Value;
        _tokenCounter = tokenCounter;
    }

    public async Task<GeneratedEmbeddings<TEmbedding>> GenerateAsync(
        IEnumerable<TInput> inputs,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var inputList = inputs.ToList();
        var metadata = _inner.GetService<EmbeddingGeneratorMetadata>();
        var model = options?.ModelId ?? metadata?.DefaultModelId ?? "unknown";
        var provider = metadata?.ProviderName ?? "extensions.ai";

        using var activity = SmartLLMTelemetryActivitySource.Instance.StartActivity(
            SmartLLMTelemetryActivitySource.Operations.Embeddings,
            ActivityKind.Client);

        activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.Provider, provider);
        activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.ModelName, model);
        activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.Operation, "embeddings");
        activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.InputCount, inputList.Count);

        if (_options.DefaultTenantId is not null)
        {
            activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.TenantId, _options.DefaultTenantId);
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await _inner.GenerateAsync(inputList, options, cancellationToken).ConfigureAwait(false);
            stopwatch.Stop();

            var dimensions = 0;
            if (result.FirstOrDefault() is Embedding<float> floatEmbedding)
            {
                dimensions = floatEmbedding.Vector.Length;
            }

            activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.EmbeddingDimensions, dimensions);

            var inputTexts = inputList.Select(i => i?.ToString() ?? string.Empty).ToArray();
            var tokenEstimate = _tokenCounter.EstimateUsage(model, inputTexts, completion: null);
            var usage = new LlmUsage
            {
                PromptTokens = tokenEstimate.PromptTokens,
                CompletionTokens = 0,
                EstimatedCostUsd = tokenEstimate.EstimatedCostUsd,
                IsEstimated = true
            };

            InstrumentedLlmClient.ApplyUsageTags(activity, usage);
            activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.DurationMs, stopwatch.Elapsed.TotalMilliseconds);
            activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.Status, SmartLLMTelemetryActivitySource.StatusValues.Ok);
            activity?.SetStatus(ActivityStatusCode.Ok);

            SmartLLMTelemetryMetrics.RecordEmbedding(
                provider,
                model,
                SmartLLMTelemetryActivitySource.StatusValues.Ok,
                inputList.Count,
                usage.TotalTokens,
                stopwatch.Elapsed.TotalMilliseconds,
                usage.EstimatedCostUsd);

            return result;
        }
        catch (OperationCanceledException ex)
        {
            stopwatch.Stop();
            activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.Status, SmartLLMTelemetryActivitySource.StatusValues.Cancelled);
            activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.DurationMs, stopwatch.Elapsed.TotalMilliseconds);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.Status, SmartLLMTelemetryActivitySource.StatusValues.Error);
            activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.DurationMs, stopwatch.Elapsed.TotalMilliseconds);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
        => _inner.GetService(serviceType, serviceKey);

    public void Dispose()
    {
        if (_inner is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
