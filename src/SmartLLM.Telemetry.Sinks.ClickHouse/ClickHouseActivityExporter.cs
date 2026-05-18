using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.OpenTelemetry;

namespace SmartLLM.Telemetry.Sinks.ClickHouse;

/// <summary>Exports completed SmartLLM activities to the ClickHouse sink.</summary>
public sealed class ClickHouseActivityExporter : IDisposable
{
    private readonly ActivityListener _listener;
    private readonly IClickHouseTelemetrySink _sink;
    private readonly SmartLLMTelemetryOptions _options;
    private readonly ClickHouseSinkOptions _sinkOptions;
    private readonly IContentRedactor? _contentRedactor;
    private readonly ILogger<ClickHouseActivityExporter>? _logger;

    public ClickHouseActivityExporter(
        IClickHouseTelemetrySink sink,
        IOptions<SmartLLMTelemetryOptions> options,
        IOptions<ClickHouseSinkOptions> sinkOptions,
        ILogger<ClickHouseActivityExporter>? logger = null,
        IContentRedactor? contentRedactor = null)
    {
        _sink = sink;
        _options = options.Value;
        _sinkOptions = sinkOptions.Value;
        _contentRedactor = _sinkOptions.RedactExportedContent ? contentRedactor : null;
        _logger = logger;
        _listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == SmartLLMTelemetryActivitySource.Name,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStopped = OnActivityStopped
        };
        ActivitySource.AddActivityListener(_listener);
    }

    private void OnActivityStopped(Activity activity)
    {
        if (activity.Duration == TimeSpan.Zero)
        {
            return;
        }

        try
        {
            var trace = ClickHouseActivityMapper.MapTrace(activity, _options, _contentRedactor);
            _sink.EnqueueTraceAsync(trace).AsTask().GetAwaiter().GetResult();

            foreach (var log in ClickHouseActivityMapper.MapLogs(activity, _contentRedactor))
            {
                _sink.EnqueueLogAsync(log).AsTask().GetAwaiter().GetResult();
            }

            var cost = ClickHouseActivityMapper.MapCost(activity, _options);
            if (cost is not null)
            {
                _sink.EnqueueCostAsync(cost).AsTask().GetAwaiter().GetResult();
            }

            _logger?.LogDebug(
                "Enqueued ClickHouse rows for {TraceId} model={Model} tokens={Tokens}",
                trace.TraceId,
                trace.ModelName,
                trace.TotalTokens);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to enqueue ClickHouse rows for {Operation}", activity.OperationName);
        }
    }

    [Obsolete("Use ClickHouseActivityMapper.MapTrace")]
    internal static TraceRow MapRow(Activity activity, SmartLLMTelemetryOptions? options = null)
        => ClickHouseActivityMapper.MapTrace(activity, options);

    public void Dispose() => _listener.Dispose();
}
