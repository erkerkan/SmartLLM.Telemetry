using SmartLLM.Telemetry.Core;

namespace SmartLLM.Telemetry.Security;

/// <summary>Redacts PII in execution context before export-oriented processing.</summary>
public sealed class PiiRedactionInterceptor : ILlmInterceptor
{
    private readonly PiiRedactor _redactor;
    private readonly PiiRedactionOptions _options;

    public PiiRedactionInterceptor(PiiRedactor redactor, Microsoft.Extensions.Options.IOptions<PiiRedactionOptions> options)
    {
        _redactor = redactor;
        _options = options.Value;
    }

    public int Order => 10;

    public ValueTask OnExecutingAsync(LlmExecutionContext context, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || !_options.RedactInPrompts)
        {
            return ValueTask.CompletedTask;
        }

        context.Items["smartllm.redacted.prompt"] = _redactor.Redact(
            string.Join("\n", context.Request.Messages.Select(m => m.Content)));
        return ValueTask.CompletedTask;
    }

    public ValueTask OnExecutedAsync(LlmExecutionContext context, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || !_options.RedactInCompletions || context.Response is null)
        {
            return ValueTask.CompletedTask;
        }

        context.Items["smartllm.redacted.completion"] = _redactor.Redact(context.Response.Content);
        return ValueTask.CompletedTask;
    }
}
