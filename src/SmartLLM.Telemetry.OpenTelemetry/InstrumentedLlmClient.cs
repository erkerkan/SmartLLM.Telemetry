using System.Diagnostics;
using Microsoft.Extensions.Options;
using SmartLLM.Telemetry.Core;

namespace SmartLLM.Telemetry.OpenTelemetry;

/// <summary>Wraps <see cref="ILlmClient"/> with ActivitySource instrumentation.</summary>
public sealed class InstrumentedLlmClient : ILlmClient
{
    private readonly ILlmClient _inner;
    private readonly SmartLLMTelemetryOptions _options;

    public InstrumentedLlmClient(ILlmClient inner, IOptions<SmartLLMTelemetryOptions> options)
    {
        _inner = inner;
        _options = options.Value;
    }

    public string Provider => _inner.Provider;

    public async Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        var requestId = request.RequestId ?? Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
        using var activity = SmartLLMTelemetryActivitySource.Instance.StartActivity(
            SmartLLMTelemetryActivitySource.Operations.Chat,
            ActivityKind.Client);

        activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.Provider, _inner.Provider);
        activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.ModelName, request.Model);
        activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.Operation, "chat");
        activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.RequestId, requestId);

        if (request.TenantId is not null)
        {
            activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.TenantId, request.TenantId);
        }
        else if (_options.DefaultTenantId is not null)
        {
            activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.TenantId, _options.DefaultTenantId);
        }

        if (request.ApiKeyId is not null)
        {
            activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.ApiKeyId, request.ApiKeyId);
        }

        if (_options.CapturePrompts)
        {
            var prompt = string.Join("\n", request.Messages.Select(m => $"{m.Role}: {m.Content}"));
            activity?.AddEvent(new ActivityEvent("smartllm.prompt", tags: new ActivityTagsCollection
            {
                ["content"] = prompt
            }));
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await _inner.CompleteAsync(request, cancellationToken).ConfigureAwait(false);
            stopwatch.Stop();

            ApplyUsageTags(activity, response.Usage);
            activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.DurationMs, stopwatch.Elapsed.TotalMilliseconds);
            activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.Status, SmartLLMTelemetryActivitySource.StatusValues.Ok);
            activity?.SetStatus(ActivityStatusCode.Ok);

            if (_options.CaptureCompletions)
            {
                activity?.AddEvent(new ActivityEvent("smartllm.completion", tags: new ActivityTagsCollection
                {
                    ["content"] = response.Content
                }));
            }

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

    public static void ApplyUsageTags(Activity? activity, LlmUsage? usage)
    {
        if (activity is null || usage is null)
        {
            return;
        }

        activity.SetTag(SmartLLMTelemetryActivitySource.Tags.PromptTokens, usage.PromptTokens);
        activity.SetTag(SmartLLMTelemetryActivitySource.Tags.CompletionTokens, usage.CompletionTokens);
        activity.SetTag(SmartLLMTelemetryActivitySource.Tags.TotalTokens, usage.TotalTokens);

        if (usage.EstimatedCostUsd is double cost)
        {
            activity.SetTag(SmartLLMTelemetryActivitySource.Tags.EstimatedCostUsd, cost);
        }
    }
}
