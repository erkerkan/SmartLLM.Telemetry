using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SmartLLM.Telemetry.Sinks.ClickHouse;

/// <summary>
/// Batched ClickHouse writer skeleton. HTTP insert wiring lands in 0.2.0.
/// </summary>
public sealed class ClickHouseTelemetrySink : BackgroundService, IClickHouseTelemetrySink
{
    private readonly Channel<TraceRow> _channel;
    private readonly ClickHouseSinkOptions _options;
    private readonly ILogger<ClickHouseTelemetrySink> _logger;

    public ClickHouseTelemetrySink(
        IOptions<ClickHouseSinkOptions> options,
        ILogger<ClickHouseTelemetrySink> logger)
    {
        _options = options.Value;
        _logger = logger;
        _channel = Channel.CreateBounded<TraceRow>(new BoundedChannelOptions(_options.BatchSize * 2)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });
    }

    public ValueTask EnqueueAsync(TraceRow row, CancellationToken cancellationToken = default)
        => _channel.Writer.WriteAsync(row, cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            _logger.LogInformation("ClickHouse connection string not configured; sink is disabled.");
            return;
        }

        var batch = new List<TraceRow>(_options.BatchSize);
        using var timer = new PeriodicTimer(_options.FlushInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                while (_channel.Reader.TryRead(out var row))
                {
                    batch.Add(row);
                    if (batch.Count >= _options.BatchSize)
                    {
                        await FlushBatchAsync(batch, stoppingToken).ConfigureAwait(false);
                        batch.Clear();
                    }
                }

                if (batch.Count > 0 && await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
                {
                    await FlushBatchAsync(batch, stoppingToken).ConfigureAwait(false);
                    batch.Clear();
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private Task FlushBatchAsync(List<TraceRow> batch, CancellationToken cancellationToken)
    {
        // Placeholder: integrate ClickHouse.Client or HTTP bulk insert.
        _logger.LogDebug("ClickHouse flush placeholder for {Count} trace rows.", batch.Count);
        return Task.CompletedTask;
    }
}

/// <summary>Sink enqueue API.</summary>
public interface IClickHouseTelemetrySink
{
    ValueTask EnqueueAsync(TraceRow row, CancellationToken cancellationToken = default);
}
