using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SmartLLM.Telemetry.Sinks.ClickHouse;

/// <summary>DI registration for ClickHouse sink.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSmartLLMClickHouseSink(
        this IServiceCollection services,
        Action<ClickHouseSinkOptions>? configure = null)
    {
        services.AddOptions<ClickHouseSinkOptions>();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        // Single instance: exporter enqueues to the same channel the hosted service drains.
        services.AddSingleton<ClickHouseTelemetrySink>();
        services.AddSingleton<IClickHouseTelemetrySink>(sp => sp.GetRequiredService<ClickHouseTelemetrySink>());
        services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<ClickHouseTelemetrySink>());
        services.AddSingleton<ClickHouseActivityExporter>();
        services.AddSingleton<IHostedService, ClickHouseActivityExporterActivator>();
        return services;
    }
}
