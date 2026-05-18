using System.Net.Sockets;
using Microsoft.Extensions.Options;
using SmartLLM.Telemetry.Sinks.ClickHouse;
using Xunit;

namespace SmartLLM.Telemetry.Sinks.ClickHouse.Tests;

public sealed class ClickHouseRetryPolicyTests
{
    [Fact]
    public async Task ExecuteAsync_retries_transient_failures()
    {
        var options = Options.Create(new ClickHouseSinkOptions
        {
            MaxRetryAttempts = 3,
            InitialRetryDelay = TimeSpan.FromMilliseconds(1)
        });
        var policy = new ClickHouseRetryPolicy(options);
        var attempts = 0;

        await policy.ExecuteAsync(_ =>
        {
            attempts++;
            if (attempts < 3)
            {
                throw new ClickHouseInsertException("transient", 503, "unavailable");
            }

            return Task.CompletedTask;
        }, CancellationToken.None);

        Assert.Equal(3, attempts);
    }

    [Fact]
    public async Task ExecuteAsync_does_not_retry_client_errors()
    {
        var options = Options.Create(new ClickHouseSinkOptions { MaxRetryAttempts = 3 });
        var policy = new ClickHouseRetryPolicy(options);
        var attempts = 0;

        await Assert.ThrowsAsync<ClickHouseInsertException>(() =>
            policy.ExecuteAsync(_ =>
            {
                attempts++;
                throw new ClickHouseInsertException("bad request", 400, "syntax error");
            }, CancellationToken.None));

        Assert.Equal(1, attempts);
    }

    [Fact]
    public async Task ExecuteAsync_retries_http_connection_reset()
    {
        var options = Options.Create(new ClickHouseSinkOptions
        {
            MaxRetryAttempts = 3,
            InitialRetryDelay = TimeSpan.FromMilliseconds(1)
        });
        var policy = new ClickHouseRetryPolicy(options);
        var attempts = 0;

        await policy.ExecuteAsync(_ =>
        {
            attempts++;
            if (attempts < 2)
            {
                throw new HttpRequestException(
                    "connection reset",
                    new IOException("reset", new SocketException((int)SocketError.ConnectionReset)));
            }

            return Task.CompletedTask;
        }, CancellationToken.None);

        Assert.Equal(2, attempts);
    }

    [Fact]
    public void IsRetriable_treats_connection_reset_as_transient()
    {
        var ex = new HttpRequestException(
            "reset",
            new IOException("io", new SocketException((int)SocketError.ConnectionReset)));

        Assert.True(ClickHouseRetryPolicy.IsRetriable(ex));
    }
}
