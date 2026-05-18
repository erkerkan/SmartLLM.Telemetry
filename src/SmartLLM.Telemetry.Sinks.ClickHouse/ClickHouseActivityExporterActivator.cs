using Microsoft.Extensions.Hosting;

namespace SmartLLM.Telemetry.Sinks.ClickHouse;

/// <summary>Eager-starts <see cref="ClickHouseActivityExporter"/> so the ActivityListener is registered.</summary>
internal sealed class ClickHouseActivityExporterActivator : IHostedService
{
    public ClickHouseActivityExporterActivator(ClickHouseActivityExporter exporter)
    {
        _ = exporter;
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
