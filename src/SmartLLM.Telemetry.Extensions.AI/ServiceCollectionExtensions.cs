using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SmartLLM.Telemetry.Extensions.AI;

/// <summary>DI helpers for Microsoft.Extensions.AI instrumentation.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Decorates all registered <see cref="IChatClient"/> instances with telemetry.</summary>
    public static IServiceCollection AddSmartLLMChatClientInstrumentation(this IServiceCollection services)
    {
        services.TryAddSingleton<InstrumentedChatClientFactory>();
        return services;
    }

    /// <summary>Wraps an existing chat client instance.</summary>
    public static IChatClient WithSmartLLMTelemetry(this IChatClient inner, IServiceProvider services)
        => services.GetRequiredService<InstrumentedChatClientFactory>().Create(inner);

    /// <summary>Registers <typeparamref name="TClient"/> as instrumented <see cref="IChatClient"/>.</summary>
    public static IServiceCollection AddInstrumentedChatClient<TClient>(this IServiceCollection services)
        where TClient : class, IChatClient
    {
        services.AddSingleton<TClient>();
        services.AddSingleton<IChatClient>(sp =>
            sp.GetRequiredService<InstrumentedChatClientFactory>().Create(sp.GetRequiredService<TClient>()));
        return services;
    }
}

/// <summary>Factory for wrapping chat clients.</summary>
public sealed class InstrumentedChatClientFactory
{
    private readonly IServiceProvider _services;

    public InstrumentedChatClientFactory(IServiceProvider services)
    {
        _services = services;
    }

    public IChatClient Create(IChatClient inner)
        => ActivatorUtilities.CreateInstance<InstrumentedChatClient>(_services, inner);
}
