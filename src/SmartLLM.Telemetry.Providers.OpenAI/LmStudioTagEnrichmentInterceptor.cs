using System.Diagnostics;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.OpenTelemetry;

namespace SmartLLM.Telemetry.Providers.OpenAI;

/// <summary>Overrides provider tag for LM Studio (OpenAI-compatible local server).</summary>
public sealed class LmStudioTagEnrichmentInterceptor : ILlmInterceptor
{
    public int Order => 210;

    public ValueTask OnExecutingAsync(LlmExecutionContext context, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    public ValueTask OnExecutedAsync(LlmExecutionContext context, CancellationToken cancellationToken = default)
    {
        Activity.Current?.SetTag(SmartLLMTelemetryActivitySource.Tags.Provider, "lmstudio");
        return ValueTask.CompletedTask;
    }
}
