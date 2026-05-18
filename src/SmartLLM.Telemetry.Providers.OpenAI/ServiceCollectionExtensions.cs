using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.Extensions.AI;
using SmartLLM.Telemetry.OpenTelemetry;

namespace SmartLLM.Telemetry.Providers.OpenAI;

/// <summary>DI registration for OpenAI provider.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers OpenAI <see cref="IChatClient"/>, instrumented chat client, and <see cref="ILlmClient"/> pipeline.
    /// Uses <c>OPENAI_API_KEY</c> when <see cref="OpenAiProviderOptions.ApiKey"/> is not set.
    /// </summary>
    public static IServiceCollection AddSmartLLMOpenAI(
        this IServiceCollection services,
        Action<OpenAiProviderOptions>? configure = null)
    {
        services.AddOptions<OpenAiProviderOptions>();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddSingleton<OpenAiChatClientHolder>();
        services.AddSmartLLMChatClientInstrumentation();
        services.TryAddSingleton<IChatClient>(sp =>
            sp.GetRequiredService<InstrumentedChatClientFactory>()
                .Create(sp.GetRequiredService<OpenAiChatClientHolder>().RawClient));

        services.AddLlmInterceptor<OpenAiTagEnrichmentInterceptor>();
        services.AddInstrumentedLlmClient<OpenAiLlmClient>();
        return services;
    }

    /// <summary>Registers only the raw OpenAI chat client (no SmartLLM instrumentation).</summary>
    public static IServiceCollection AddOpenAIChatClient(
        this IServiceCollection services,
        Action<OpenAiProviderOptions>? configure = null)
    {
        services.AddOptions<OpenAiProviderOptions>();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddSingleton<OpenAiChatClientHolder>();
        services.TryAddSingleton<IChatClient>(sp => sp.GetRequiredService<OpenAiChatClientHolder>().RawClient);
        return services;
    }
}

/// <summary>Holds the non-instrumented OpenAI chat client instance.</summary>
public sealed class OpenAiChatClientHolder
{
    public OpenAiChatClientHolder(Microsoft.Extensions.Options.IOptions<OpenAiProviderOptions> options)
    {
        RawClient = OpenAiChatClientFactory.Create(options);
    }

    public IChatClient RawClient { get; }
}
