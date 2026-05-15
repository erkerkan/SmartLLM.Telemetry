using Microsoft.Extensions.DependencyInjection;
using SmartLLM.Telemetry.Core;

namespace SmartLLM.Telemetry.Tokenizer;

/// <summary>DI registration for tokenizer services.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSmartLLMTokenizer(this IServiceCollection services)
    {
        services.AddSingleton<IModelPricingTable, ModelPricingTable>();
        services.AddSingleton<ITokenCounter, OfflineTokenCounter>();
        services.AddLlmInterceptor<TokenEstimationInterceptor>();
        return services;
    }
}
