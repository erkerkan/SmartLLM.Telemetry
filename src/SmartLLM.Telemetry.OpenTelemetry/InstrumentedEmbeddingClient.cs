using System.Diagnostics;
using Microsoft.Extensions.Options;
using SmartLLM.Telemetry.Core;

namespace SmartLLM.Telemetry.OpenTelemetry;

/// <summary>Wraps <see cref="IEmbeddingClient"/> with ActivitySource instrumentation.</summary>
public sealed class InstrumentedEmbeddingClient : IEmbeddingClient
{
    private readonly IEmbeddingClient _inner;
    private readonly SmartLLMTelemetryOptions _options;

    public InstrumentedEmbeddingClient(IEmbeddingClient inner, IOptions<SmartLLMTelemetryOptions> options)
    {
        _inner = inner;
        _options = options.Value;
    }

    public string Provider => _inner.Provider;

    public async Task<EmbeddingResponse> EmbedAsync(EmbeddingRequest request, CancellationToken cancellationToken = default)
    {
        var requestId = request.RequestId ?? Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        using var activity = SmartLLMTelemetryActivitySource.Instance.StartActivity(
            SmartLLMTelemetryActivitySource.Operations.Embeddings,
            ActivityKind.Client);

        activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.Provider, _inner.Provider);
        activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.ModelName, request.Model);
        activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.Operation, "embeddings");
        activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.RequestId, requestId);
        activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.InputCount, request.Inputs.Count);

        if (request.TenantId is not null)
        {
            activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.TenantId, request.TenantId);
        }
        else if (_options.DefaultTenantId is not null)
        {
            activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.TenantId, _options.DefaultTenantId);
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await _inner.EmbedAsync(request, cancellationToken).ConfigureAwait(false);
            stopwatch.Stop();

            activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.EmbeddingDimensions, response.Dimensions);
            InstrumentedLlmClient.ApplyUsageTags(activity, response.Usage);
            activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.DurationMs, stopwatch.Elapsed.TotalMilliseconds);
            activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.Status, SmartLLMTelemetryActivitySource.StatusValues.Ok);
            activity?.SetStatus(ActivityStatusCode.Ok);

            SmartLLMTelemetryMetrics.RecordEmbedding(
                _inner.Provider,
                request.Model,
                SmartLLMTelemetryActivitySource.StatusValues.Ok,
                request.Inputs.Count,
                response.Usage?.TotalTokens ?? 0,
                stopwatch.Elapsed.TotalMilliseconds,
                response.Usage?.EstimatedCostUsd);

            return response;
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
}
