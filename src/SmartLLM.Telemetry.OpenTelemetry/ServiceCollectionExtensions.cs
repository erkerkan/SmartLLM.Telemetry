using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;
using SmartLLM.Telemetry.Core;

namespace SmartLLM.Telemetry.OpenTelemetry;

/// <summary>DI registration for OpenTelemetry instrumentation.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <typeparamref name="TClient"/> and exposes an instrumented, interceptor-wrapped <see cref="ILlmClient"/>.
    /// </summary>
    public static IServiceCollection AddInstrumentedLlmClient<TClient>(this IServiceCollection services)
        where TClient : class, ILlmClient
    {
        services.AddSingleton<TClient>();
        services.AddSingleton<ILlmClient>(sp =>
        {
            var inner = sp.GetRequiredService<TClient>();
            var interceptors = sp.GetServices<ILlmInterceptor>();
            var pipeline = new LlmInterceptorPipeline(inner, interceptors);
            var options = sp.GetRequiredService<IOptions<SmartLLMTelemetryOptions>>();
            return new InstrumentedLlmClient(pipeline, options);
        });
        return services;
    }

    public static IServiceCollection AddConsoleTraceExporter(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder.AddSource(SmartLLMTelemetryActivitySource.Name);
                builder.AddConsoleExporter();
            });
        return services;
    }
}
