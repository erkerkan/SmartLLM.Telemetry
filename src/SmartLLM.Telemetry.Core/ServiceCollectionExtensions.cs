using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SmartLLM.Telemetry.Core;

/// <summary>DI registration for SmartLLM.Telemetry.Core.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSmartLLMTelemetry(
        this IServiceCollection services,
        Action<SmartLLMTelemetryOptions>? configure = null)
    {
        services.AddOptions<SmartLLMTelemetryOptions>();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddEnumerable(ServiceDescriptor.Singleton<ILlmInterceptor, NoOpInterceptor>());
        return services;
    }

    public static IServiceCollection AddLlmInterceptor<TInterceptor>(this IServiceCollection services)
        where TInterceptor : class, ILlmInterceptor
    {
        services.AddSingleton<ILlmInterceptor, TInterceptor>();
        return services;
    }

    public static IServiceCollection AddLlmClient<TClient>(this IServiceCollection services)
        where TClient : class, ILlmClient
    {
        services.AddSingleton<ILlmClient>(sp =>
        {
            var inner = sp.GetRequiredService<TClient>();
            var interceptors = sp.GetServices<ILlmInterceptor>();
            return new LlmInterceptorPipeline(inner, interceptors);
        });
        services.AddSingleton<TClient>();
        return services;
    }

    private sealed class NoOpInterceptor : ILlmInterceptor
    {
        public ValueTask OnExecutingAsync(LlmExecutionContext context, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;

        public ValueTask OnExecutedAsync(LlmExecutionContext context, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;
    }
}
