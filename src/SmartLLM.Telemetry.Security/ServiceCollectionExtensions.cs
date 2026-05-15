using Microsoft.Extensions.DependencyInjection;
using SmartLLM.Telemetry.Core;

namespace SmartLLM.Telemetry.Security;

/// <summary>DI registration for security services.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSmartLLMSecurity(
        this IServiceCollection services,
        Action<PiiRedactionOptions>? configure = null)
    {
        services.AddOptions<PiiRedactionOptions>();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.AddSingleton<PiiRedactor>();
        services.AddLlmInterceptor<PiiRedactionInterceptor>();
        return services;
    }
}
