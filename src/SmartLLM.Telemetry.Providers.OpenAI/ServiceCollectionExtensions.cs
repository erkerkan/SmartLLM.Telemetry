using Microsoft.Extensions.DependencyInjection;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.OpenTelemetry;

namespace SmartLLM.Telemetry.Providers.OpenAI;

/// <summary>DI registration for OpenAI provider.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSmartLLMOpenAI(this IServiceCollection services)
    {
        services.AddLlmInterceptor<OpenAiTagEnrichmentInterceptor>();
        services.AddInstrumentedLlmClient<OpenAiLlmClient>();
        return services;
    }
}
