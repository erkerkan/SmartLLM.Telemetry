using System.Diagnostics;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.OpenTelemetry;

namespace SmartLLM.Telemetry.Providers.OpenAI;

/// <summary>Enriches the current activity with OpenAI-specific metadata after execution.</summary>
public sealed class OpenAiTagEnrichmentInterceptor : ILlmInterceptor
{
    public int Order => 200;

    public ValueTask OnExecutingAsync(LlmExecutionContext context, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    public ValueTask OnExecutedAsync(LlmExecutionContext context, CancellationToken cancellationToken = default)
    {
        var activity = Activity.Current;
        if (activity is null)
        {
            return ValueTask.CompletedTask;
        }

        activity.SetTag(SmartLLMTelemetryActivitySource.Tags.Provider, "openai");
        activity.SetTag(SmartLLMTelemetryActivitySource.Tags.ModelName, context.Request.Model);

        var usage = context.Usage ?? context.Response?.Usage;
        InstrumentedLlmClient.ApplyUsageTags(activity, usage);
        return ValueTask.CompletedTask;
    }
}
