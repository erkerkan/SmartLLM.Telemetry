using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SmartLLM.Telemetry.Sinks.ClickHouse;

/// <summary>Executes ClickHouse writes with configurable retry and backoff.</summary>
internal sealed class ClickHouseRetryPolicy
{
    private readonly ClickHouseSinkOptions _options;
    private readonly ILogger? _logger;

    public ClickHouseRetryPolicy(IOptions<ClickHouseSinkOptions> options, ILogger? logger = null)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        var maxAttempts = Math.Max(1, _options.MaxRetryAttempts);
        var delay = _options.InitialRetryDelay;
        Exception? lastError = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await action(cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (Exception ex) when (IsRetriable(ex))
            {
                lastError = ex;
                if (attempt >= maxAttempts)
                {
                    throw;
                }

                _logger?.LogWarning(
                    ex,
                    "ClickHouse write failed (attempt {Attempt}/{MaxAttempts}); retrying in {Delay}ms",
                    attempt,
                    maxAttempts,
                    delay.TotalMilliseconds);

                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 2, 30_000));
            }
        }

        if (lastError is not null)
        {
            throw lastError;
        }
    }

    internal static bool IsRetriable(Exception ex)
    {
        if (ex is ClickHouseInsertException insert)
        {
            return insert.StatusCode >= 500 || insert.StatusCode == 429;
        }

        if (ex is HttpRequestException or IOException or SocketException)
        {
            return true;
        }

        return ex.InnerException is not null && IsRetriable(ex.InnerException);
    }
}
