namespace SmartLLM.Telemetry.Core;

/// <summary>Decorates an <see cref="ILlmClient"/> with ordered interceptors.</summary>
public sealed class LlmInterceptorPipeline : ILlmClient
{
    private readonly ILlmClient _inner;
    private readonly IReadOnlyList<ILlmInterceptor> _interceptors;

    public LlmInterceptorPipeline(ILlmClient inner, IEnumerable<ILlmInterceptor> interceptors)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _interceptors = interceptors?.OrderBy(i => i.Order).ToArray()
            ?? throw new ArgumentNullException(nameof(interceptors));
    }

    public string Provider => _inner.Provider;

    public async Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        var context = new LlmExecutionContext { Request = request };

        foreach (var interceptor in _interceptors)
        {
            await interceptor.OnExecutingAsync(context, cancellationToken).ConfigureAwait(false);
        }

        try
        {
            var response = await _inner.CompleteAsync(request, cancellationToken).ConfigureAwait(false);
            context.Response = response;
            context.Usage = response.Usage ?? context.Usage;
        }
        catch (Exception ex)
        {
            context.Exception = ex;
            throw;
        }
        finally
        {
            for (var i = _interceptors.Count - 1; i >= 0; i--)
            {
                await _interceptors[i].OnExecutedAsync(context, cancellationToken).ConfigureAwait(false);
            }
        }

        if (context.Response is not null
            && context.Usage is not null
            && context.Response.Usage is null)
        {
            var response = context.Response;
            context.Response = new LlmResponse
            {
                Content = response.Content,
                Model = response.Model,
                FinishReason = response.FinishReason,
                Duration = response.Duration,
                Usage = context.Usage
            };
        }

        return context.Response!;
    }
}
