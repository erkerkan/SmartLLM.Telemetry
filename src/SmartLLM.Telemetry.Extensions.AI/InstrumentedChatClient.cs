using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.OpenTelemetry;
using SmartLLM.Telemetry.Tokenizer;

namespace SmartLLM.Telemetry.Extensions.AI;

/// <summary>Decorates <see cref="IChatClient"/> with OpenTelemetry LLM instrumentation.</summary>
public sealed class InstrumentedChatClient : IChatClient, IDisposable
{
    private readonly IChatClient _inner;
    private readonly SmartLLMTelemetryOptions _telemetryOptions;
    private readonly ITokenCounter _tokenCounter;

    public InstrumentedChatClient(
        IChatClient inner,
        IOptions<SmartLLMTelemetryOptions> telemetryOptions,
        ITokenCounter tokenCounter)
    {
        _inner = inner;
        _telemetryOptions = telemetryOptions.Value;
        _tokenCounter = tokenCounter;
    }

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var messageList = messages.ToList();
        var model = ResolveModel(options);
        using var activity = StartActivity(model);

        try
        {
            var response = await _inner.GetResponseAsync(messageList, options, cancellationToken).ConfigureAwait(false);
            EnrichActivity(activity, model, messageList, response);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return response;
        }
        catch (OperationCanceledException ex)
        {
            SetCancelled(activity, ex);
            throw;
        }
        catch (Exception ex)
        {
            SetError(activity, ex);
            throw;
        }
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messageList = messages.ToList();
        var model = ResolveModel(options);
        using var activity = StartActivity(model, operation: "chat_stream");
        var stopwatch = Stopwatch.StartNew();
        var completionBuilder = new StringBuilder();

        await foreach (var update in _inner.GetStreamingResponseAsync(messageList, options, cancellationToken)
                           .WithCancellation(cancellationToken))
        {
            AppendStreamingText(completionBuilder, update);
            yield return update;
        }

        stopwatch.Stop();
        var completionText = completionBuilder.ToString();
        var response = ChatClientLlmBridge.ToChatResponse(messageList, completionText, usage: null);
        EnrichActivity(activity, model, messageList, response, completionText);
        activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.DurationMs, stopwatch.Elapsed.TotalMilliseconds);
        activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.Status, SmartLLMTelemetryActivitySource.StatusValues.Ok);
        activity?.SetStatus(ActivityStatusCode.Ok);
    }

    private static void AppendStreamingText(StringBuilder builder, ChatResponseUpdate update)
    {
        if (!string.IsNullOrEmpty(update.Text))
        {
            builder.Append(update.Text);
        }

        if (update.Contents is null)
        {
            return;
        }

        foreach (var content in update.Contents)
        {
            if (content is TextContent text && !string.IsNullOrEmpty(text.Text))
            {
                builder.Append(text.Text);
            }
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
        => _inner.GetService(serviceType, serviceKey);

    public void Dispose()
    {
        if (_inner is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private Activity? StartActivity(string model, string operation = "chat")
    {
        var activity = SmartLLMTelemetryActivitySource.Instance.StartActivity(
            SmartLLMTelemetryActivitySource.Operations.Chat,
            ActivityKind.Client);

        if (activity is null)
        {
            return null;
        }

        activity.SetTag(SmartLLMTelemetryActivitySource.Tags.Provider, ResolveProviderName());
        activity.SetTag(SmartLLMTelemetryActivitySource.Tags.ModelName, model);
        activity.SetTag(SmartLLMTelemetryActivitySource.Tags.Operation, operation);
        activity.SetTag(SmartLLMTelemetryActivitySource.Tags.RequestId,
            Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N"));

        if (_telemetryOptions.DefaultTenantId is not null)
        {
            activity.SetTag(SmartLLMTelemetryActivitySource.Tags.TenantId, _telemetryOptions.DefaultTenantId);
        }

        return activity;
    }

    private void EnrichActivity(
        Activity? activity,
        string model,
        IReadOnlyList<ChatMessage> messages,
        ChatResponse response,
        string? streamedCompletion = null)
    {
        if (activity is null)
        {
            return;
        }

        if (_telemetryOptions.CapturePrompts)
        {
            AddPromptEvent(activity, messages);
        }

        var usage = ResolveUsage(model, messages, response);
        InstrumentedLlmClient.ApplyUsageTags(activity, usage);
        activity.SetTag(SmartLLMTelemetryActivitySource.Tags.Status, SmartLLMTelemetryActivitySource.StatusValues.Ok);

        var completion = streamedCompletion ?? response.Text;
        if (_telemetryOptions.CaptureCompletions && completion is not null)
        {
            activity.AddEvent(new ActivityEvent("smartllm.completion", tags: new ActivityTagsCollection
            {
                ["content"] = completion
            }));
        }
    }

    private void AddPromptEvent(Activity? activity, IReadOnlyList<ChatMessage> messages)
    {
        if (activity is null)
        {
            return;
        }

        var prompt = string.Join("\n", messages.Select(m => $"{m.Role}: {m.Text}"));
        activity.AddEvent(new ActivityEvent("smartllm.prompt", tags: new ActivityTagsCollection
        {
            ["content"] = prompt
        }));
    }

    private static void SetCancelled(Activity? activity, OperationCanceledException ex)
    {
        activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.Status, SmartLLMTelemetryActivitySource.StatusValues.Cancelled);
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
    }

    private static void SetError(Activity? activity, Exception ex)
    {
        activity?.SetTag(SmartLLMTelemetryActivitySource.Tags.Status, SmartLLMTelemetryActivitySource.StatusValues.Error);
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        activity?.AddException(ex);
    }

    private LlmUsage ResolveUsage(string model, IReadOnlyList<ChatMessage> messages, ChatResponse response)
    {
        if (response.Usage is { } u && (u.InputTokenCount > 0 || u.OutputTokenCount > 0))
        {
            var prompt = (int)(u.InputTokenCount ?? 0);
            var completion = (int)(u.OutputTokenCount ?? 0);
            return new LlmUsage
            {
                PromptTokens = prompt,
                CompletionTokens = completion,
                EstimatedCostUsd = _tokenCounter.EstimateUsage(model, [], response.Text).EstimatedCostUsd,
                IsEstimated = false
            };
        }

        var parts = messages.Select(m => m.Text ?? string.Empty).ToArray();
        var estimate = _tokenCounter.EstimateUsage(model, parts, response.Text);
        return new LlmUsage
        {
            PromptTokens = estimate.PromptTokens,
            CompletionTokens = estimate.CompletionTokens,
            EstimatedCostUsd = estimate.EstimatedCostUsd,
            IsEstimated = true
        };
    }

    private static string ResolveModel(ChatOptions? options)
        => options?.ModelId ?? "unknown";

    private string ResolveProviderName()
        => _inner.GetService<ChatClientMetadata>()?.ProviderName ?? "extensions.ai";
}
