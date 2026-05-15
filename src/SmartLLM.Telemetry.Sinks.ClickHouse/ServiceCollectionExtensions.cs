using Microsoft.Extensions.DependencyInjection;

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

        services.AddSingleton<IClickHouseTelemetrySink>(sp => sp.GetRequiredService<ClickHouseTelemetrySink>());
        services.AddHostedService<ClickHouseTelemetrySink>();
        return services;
    }
}
