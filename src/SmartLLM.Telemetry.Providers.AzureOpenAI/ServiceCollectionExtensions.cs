using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.Extensions.AI;
using SmartLLM.Telemetry.OpenTelemetry;

namespace SmartLLM.Telemetry.Providers.AzureOpenAI;

/// <summary>DI registration for Azure OpenAI provider.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Azure OpenAI <see cref="IChatClient"/>, instrumented chat client, and <see cref="ILlmClient"/> pipeline.
    /// Uses <c>AZURE_OPENAI_ENDPOINT</c>, <c>AZURE_OPENAI_API_KEY</c>, and <c>AZURE_OPENAI_DEPLOYMENT</c> when options are not set.
    /// </summary>
    public static IServiceCollection AddSmartLLMAzureOpenAI(
        this IServiceCollection services,
        Action<AzureOpenAiProviderOptions>? configure = null)
    {
        services.AddOptions<AzureOpenAiProviderOptions>();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddSingleton<AzureOpenAiChatClientHolder>();
        services.AddSmartLLMChatClientInstrumentation();
        services.TryAddSingleton<IChatClient>(sp =>
            sp.GetRequiredService<InstrumentedChatClientFactory>()
                .Create(sp.GetRequiredService<AzureOpenAiChatClientHolder>().RawClient));

        services.AddLlmInterceptor<AzureOpenAiTagEnrichmentInterceptor>();
        services.AddInstrumentedLlmClient<AzureOpenAiLlmClient>();
        return services;
    }

    /// <summary>Registers only the raw Azure OpenAI chat client (no SmartLLM instrumentation).</summary>
    public static IServiceCollection AddAzureOpenAIChatClient(
        this IServiceCollection services,
        Action<AzureOpenAiProviderOptions>? configure = null)
    {
        services.AddOptions<AzureOpenAiProviderOptions>();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddSingleton<AzureOpenAiChatClientHolder>();
        services.TryAddSingleton<IChatClient>(sp => sp.GetRequiredService<AzureOpenAiChatClientHolder>().RawClient);
        return services;
    }
}

/// <summary>Holds the non-instrumented Azure OpenAI chat client instance.</summary>
public sealed class AzureOpenAiChatClientHolder
{
    public AzureOpenAiChatClientHolder(Microsoft.Extensions.Options.IOptions<AzureOpenAiProviderOptions> options)
    {
        RawClient = AzureOpenAiChatClientFactory.Create(options);
    }

    public IChatClient RawClient { get; }
}
