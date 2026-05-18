using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.Extensions.AI;
using SmartLLM.Telemetry.OpenTelemetry;

namespace SmartLLM.Telemetry.Providers.Ollama;

/// <summary>DI registration for Ollama provider.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Ollama <see cref="IChatClient"/>, instrumented chat client, and <see cref="ILlmClient"/> pipeline.
    /// Uses <c>OLLAMA_HOST</c> and <c>OLLAMA_MODEL</c> when options are not set.
    /// </summary>
    public static IServiceCollection AddSmartLLMOllama(
        this IServiceCollection services,
        Action<OllamaProviderOptions>? configure = null)
    {
        services.AddOptions<OllamaProviderOptions>();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddSingleton<OllamaChatClientHolder>();
        services.AddSmartLLMChatClientInstrumentation();
        services.TryAddSingleton<IChatClient>(sp =>
            sp.GetRequiredService<InstrumentedChatClientFactory>()
                .Create(sp.GetRequiredService<OllamaChatClientHolder>().RawClient));

        services.AddLlmInterceptor<OllamaTagEnrichmentInterceptor>();
        services.AddInstrumentedLlmClient<OllamaLlmClient>();
        return services;
    }

    /// <summary>Registers only the raw Ollama chat client (no SmartLLM instrumentation).</summary>
    public static IServiceCollection AddOllamaChatClient(
        this IServiceCollection services,
        Action<OllamaProviderOptions>? configure = null)
    {
        services.AddOptions<OllamaProviderOptions>();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.TryAddSingleton<OllamaChatClientHolder>();
        services.TryAddSingleton<IChatClient>(sp => sp.GetRequiredService<OllamaChatClientHolder>().RawClient);
        return services;
    }
}

/// <summary>Holds the non-instrumented Ollama chat client instance.</summary>
public sealed class OllamaChatClientHolder
{
    public OllamaChatClientHolder(Microsoft.Extensions.Options.IOptions<OllamaProviderOptions> options)
    {
        RawClient = OllamaChatClientFactory.Create(options);
    }

    public IChatClient RawClient { get; }
}
