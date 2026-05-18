using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartLLM.Telemetry.Core;

namespace SmartLLM.Telemetry.Sinks.ClickHouse;

/// <summary>Batched ClickHouse writer for traces, logs, and costs.</summary>
public sealed class ClickHouseTelemetrySink : BackgroundService, IClickHouseTelemetrySink
{
    private readonly Channel<TraceRow> _traceChannel;
    private readonly Channel<LogRow> _logChannel;
    private readonly Channel<CostRow> _costChannel;
    private readonly ClickHouseSinkOptions _options;
    private readonly SmartLLMTelemetryOptions _telemetryOptions;
    private readonly ILogger<ClickHouseTelemetrySink> _logger;
    private readonly ILoggerFactory? _loggerFactory;
    private ClickHouseBatchWriter? _writer;
    private ClickHouseRetryPolicy? _retry;

    public ClickHouseTelemetrySink(
        IOptions<ClickHouseSinkOptions> options,
        IOptions<SmartLLMTelemetryOptions> telemetryOptions,
        ILogger<ClickHouseTelemetrySink> logger,
        ILoggerFactory? loggerFactory = null)
    {
        _options = options.Value;
        _telemetryOptions = telemetryOptions.Value;
        _logger = logger;
        _loggerFactory = loggerFactory;

        var channelOptions = new BoundedChannelOptions(_options.BatchSize * 2)
        {
            FullMode = BoundedChannelFullMode.Wait
        };

        _traceChannel = Channel.CreateBounded<TraceRow>(channelOptions);
        _logChannel = Channel.CreateBounded<LogRow>(channelOptions);
        _costChannel = Channel.CreateBounded<CostRow>(channelOptions);
    }

    public ValueTask EnqueueTraceAsync(TraceRow row, CancellationToken cancellationToken = default)
        => _traceChannel.Writer.WriteAsync(row, cancellationToken);

    public ValueTask EnqueueLogAsync(LogRow row, CancellationToken cancellationToken = default)
        => _logChannel.Writer.WriteAsync(row, cancellationToken);

    public ValueTask EnqueueCostAsync(CostRow row, CancellationToken cancellationToken = default)
        => _costChannel.Writer.WriteAsync(row, cancellationToken);

    ValueTask IClickHouseTelemetrySink.EnqueueAsync(TraceRow row, CancellationToken cancellationToken)
        => EnqueueTraceAsync(row, cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            _logger.LogInformation("ClickHouse connection string not configured; sink is disabled.");
            return;
        }

        var settings = ClickHouseConnectionSettings.Parse(_options.ConnectionString);
        _writer = new ClickHouseBatchWriter(
            settings,
            _loggerFactory?.CreateLogger<ClickHouseBatchWriter>());
        _retry = new ClickHouseRetryPolicy(
            Options.Create(_options),
            _loggerFactory?.CreateLogger<ClickHouseRetryPolicy>());

        _logger.LogInformation(
            "ClickHouse sink started -> {Host}:{Port}/{Database}",
            settings.Host,
            settings.Port,
            settings.Database);

        var traceBatch = new List<TraceRow>(_options.BatchSize);
        var logBatch = new List<LogRow>(_options.BatchSize);
        var costBatch = new List<CostRow>(_options.BatchSize);
        using var timer = new PeriodicTimer(_options.FlushInterval);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                DrainChannels(traceBatch, logBatch, costBatch);

                if (IsBatchFull(traceBatch, logBatch, costBatch))
                {
                    await FlushAllAsync(traceBatch, logBatch, costBatch, stoppingToken).ConfigureAwait(false);
                    continue;
                }

                if (HasAnyBatch(traceBatch, logBatch, costBatch))
                {
                    try
                    {
                        await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false);
                        await FlushAllAsync(traceBatch, logBatch, costBatch, stoppingToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
                else
                {
                    await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // graceful shutdown
        }
        finally
        {
            DrainChannels(traceBatch, logBatch, costBatch);
            if (HasAnyBatch(traceBatch, logBatch, costBatch))
            {
                await FlushAllAsync(traceBatch, logBatch, costBatch, CancellationToken.None).ConfigureAwait(false);
            }
        }
    }

    private void DrainChannels(List<TraceRow> traces, List<LogRow> logs, List<CostRow> costs)
    {
        while (_traceChannel.Reader.TryRead(out var trace))
        {
            traces.Add(trace);
        }

        while (_logChannel.Reader.TryRead(out var log))
        {
            logs.Add(log);
        }

        while (_costChannel.Reader.TryRead(out var cost))
        {
            costs.Add(cost);
        }
    }

    private bool IsBatchFull(List<TraceRow> traces, List<LogRow> logs, List<CostRow> costs)
        => traces.Count >= _options.BatchSize
           || logs.Count >= _options.BatchSize
           || costs.Count >= _options.BatchSize;

    private static bool HasAnyBatch(List<TraceRow> traces, List<LogRow> logs, List<CostRow> costs)
        => traces.Count > 0 || logs.Count > 0 || costs.Count > 0;

    private async Task FlushAllAsync(
        List<TraceRow> traces,
        List<LogRow> logs,
        List<CostRow> costs,
        CancellationToken cancellationToken)
    {
        if (_writer is null || _retry is null)
        {
            return;
        }

        if (traces.Count > 0)
        {
            await FlushWithRetryAsync(
                traces,
                (batch, ct) => _writer.WriteTracesAsync(batch, ct),
                "trace",
                cancellationToken).ConfigureAwait(false);
            traces.Clear();
        }

        if (logs.Count > 0)
        {
            await FlushWithRetryAsync(
                logs,
                (batch, ct) => _writer.WriteLogsAsync(batch, ct),
                "log",
                cancellationToken).ConfigureAwait(false);
            logs.Clear();
        }

        if (costs.Count > 0)
        {
            await FlushWithRetryAsync(
                costs,
                (batch, ct) => _writer.WriteCostsAsync(batch, ct),
                "cost",
                cancellationToken).ConfigureAwait(false);
            costs.Clear();
        }
    }

    private async Task FlushWithRetryAsync<T>(
        List<T> batch,
        Func<IReadOnlyList<T>, CancellationToken, Task> write,
        string kind,
        CancellationToken cancellationToken)
    {
        var snapshot = batch.ToArray();
        try
        {
            await _retry!.ExecuteAsync(ct => write(snapshot, ct), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (ex is ClickHouseInsertException insert)
            {
                _logger.LogError(
                    ex,
                    "Failed to flush {Count} {Kind} row(s) to ClickHouse after retries (HTTP {StatusCode}). Response: {ResponseBody}",
                    snapshot.Length,
                    kind,
                    insert.StatusCode,
                    insert.ResponseBody.Trim());
            }
            else
            {
                _logger.LogError(
                    ex,
                    "Failed to flush {Count} {Kind} row(s) to ClickHouse after retries. Check that ClickHouse is running on the connection string.",
                    snapshot.Length,
                    kind);
            }
        }
    }
}

/// <summary>Sink enqueue API.</summary>
public interface IClickHouseTelemetrySink
{
    ValueTask EnqueueAsync(TraceRow row, CancellationToken cancellationToken = default);

    ValueTask EnqueueTraceAsync(TraceRow row, CancellationToken cancellationToken = default);

    ValueTask EnqueueLogAsync(LogRow row, CancellationToken cancellationToken = default);

    ValueTask EnqueueCostAsync(CostRow row, CancellationToken cancellationToken = default);
}
