using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.OpenTelemetry;

namespace SmartLLM.Telemetry.Sinks.ClickHouse;

/// <summary>Maps <see cref="Activity"/> instances to ClickHouse row models.</summary>
internal static class ClickHouseActivityMapper
{
    public static TraceRow MapTrace(
        Activity activity,
        SmartLLMTelemetryOptions? options = null,
        IContentRedactor? redactor = null)
    {
        var promptTokens = GetIntTag(activity, SmartLLMTelemetryActivitySource.Tags.PromptTokens);
        var completionTokens = GetIntTag(activity, SmartLLMTelemetryActivitySource.Tags.CompletionTokens);
        var totalTokens = GetIntTag(activity, SmartLLMTelemetryActivitySource.Tags.TotalTokens)
            ?? (promptTokens + completionTokens);

        return new TraceRow
        {
            EventTime = activity.StartTimeUtc,
            TraceId = activity.TraceId.ToString(),
            SpanId = activity.SpanId.ToString(),
            ParentSpanId = NormalizeParentSpanId(activity.ParentSpanId.ToString()),
            ServiceName = options?.ServiceName ?? "smartllm-app",
            Operation = activity.GetTagItem(SmartLLMTelemetryActivitySource.Tags.Operation)?.ToString()
                ?? activity.OperationName,
            Provider = activity.GetTagItem(SmartLLMTelemetryActivitySource.Tags.Provider)?.ToString() ?? string.Empty,
            ModelName = activity.GetTagItem(SmartLLMTelemetryActivitySource.Tags.ModelName)?.ToString() ?? string.Empty,
            Status = activity.GetTagItem(SmartLLMTelemetryActivitySource.Tags.Status)?.ToString()
                ?? (activity.Status == ActivityStatusCode.Error ? "error" : "ok"),
            DurationMs = (uint)Math.Min(activity.Duration.TotalMilliseconds, uint.MaxValue),
            PromptTokens = (uint)Math.Max(0, promptTokens ?? 0),
            CompletionTokens = (uint)Math.Max(0, completionTokens ?? 0),
            TotalTokens = (uint)Math.Max(0, totalTokens ?? 0),
            EstimatedCostUsd = GetDoubleTag(activity, SmartLLMTelemetryActivitySource.Tags.EstimatedCostUsd) ?? 0,
            TenantId = activity.GetTagItem(SmartLLMTelemetryActivitySource.Tags.TenantId)?.ToString()
                ?? options?.DefaultTenantId
                ?? string.Empty,
            Attributes = ExportContentRedactor.ApplyAttributes(ActivityTagCollector.Collect(activity), redactor)
        };
    }

    public static IReadOnlyList<LogRow> MapLogs(Activity activity, IContentRedactor? redactor = null)
    {
        var logs = new List<LogRow>();
        var traceId = activity.TraceId.ToString();
        var eventTime = activity.StartTimeUtc;

        foreach (var evt in activity.Events)
        {
            if (evt.Name is not ("smartllm.prompt" or "smartllm.completion"))
            {
                continue;
            }

            var content = ExportContentRedactor.Apply(GetEventTag(evt, "content") ?? string.Empty, redactor);
            logs.Add(new LogRow
            {
                EventTime = eventTime,
                TraceId = traceId,
                Severity = evt.Name == "smartllm.prompt" ? "info" : "debug",
                Message = Truncate(content, 8192),
                Attributes = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["event"] = evt.Name,
                    ["span_id"] = activity.SpanId.ToString()
                }
            });
        }

        if (activity.Status == ActivityStatusCode.Error)
        {
            var errorMessage = activity.GetTagItem("error.message")?.ToString()
                ?? activity.StatusDescription
                ?? "error";
            logs.Add(new LogRow
            {
                EventTime = eventTime,
                TraceId = traceId,
                Severity = "error",
                Message = Truncate(ExportContentRedactor.Apply(errorMessage, redactor), 8192),
                Attributes = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["span_id"] = activity.SpanId.ToString(),
                    ["operation"] = activity.OperationName
                }
            });
        }

        return logs;
    }

    public static CostRow? MapCost(
        Activity activity,
        SmartLLMTelemetryOptions? options = null,
        ClickHouseSinkOptions? sinkOptions = null)
    {
        var totalTokens = GetIntTag(activity, SmartLLMTelemetryActivitySource.Tags.TotalTokens);
        if (totalTokens is null or 0)
        {
            return null;
        }

        var cost = GetDoubleTag(activity, SmartLLMTelemetryActivitySource.Tags.EstimatedCostUsd) ?? 0;
        if (cost == 0 && sinkOptions?.ExportZeroCostRows != true)
        {
            return null;
        }

        var promptTokens = (uint)Math.Max(0, GetIntTag(activity, SmartLLMTelemetryActivitySource.Tags.PromptTokens) ?? 0);
        var completionTokens = (uint)Math.Max(0, GetIntTag(activity, SmartLLMTelemetryActivitySource.Tags.CompletionTokens) ?? 0);

        var apiKeyId = activity.GetTagItem(SmartLLMTelemetryActivitySource.Tags.ApiKeyId)?.ToString();

        return new CostRow
        {
            EventTime = activity.StartTimeUtc,
            TenantId = activity.GetTagItem(SmartLLMTelemetryActivitySource.Tags.TenantId)?.ToString()
                ?? options?.DefaultTenantId
                ?? string.Empty,
            ApiKeyHash = HashApiKeyId(apiKeyId),
            Provider = activity.GetTagItem(SmartLLMTelemetryActivitySource.Tags.Provider)?.ToString() ?? string.Empty,
            ModelName = activity.GetTagItem(SmartLLMTelemetryActivitySource.Tags.ModelName)?.ToString() ?? string.Empty,
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
            TotalTokens = (uint)totalTokens.Value,
            CostUsd = cost,
            Currency = "USD"
        };
    }

    private static string HashApiKeyId(string? apiKeyId)
    {
        if (string.IsNullOrEmpty(apiKeyId))
        {
            return string.Empty;
        }

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(apiKeyId));
        return Convert.ToHexString(hash)[..16].ToLowerInvariant();
    }

    private static string? GetEventTag(ActivityEvent activityEvent, string key)
    {
        foreach (var tag in activityEvent.EnumerateTagObjects())
        {
            if (tag.Key == key)
            {
                return tag.Value?.ToString();
            }
        }

        return null;
    }

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength];

    private static string NormalizeParentSpanId(string parentSpanId)
        => string.IsNullOrEmpty(parentSpanId) || parentSpanId == "0000000000000000"
            ? string.Empty
            : parentSpanId;

    private static int? GetIntTag(Activity activity, string key)
        => activity.GetTagItem(key) switch
        {
            int i => i,
            long l => (int)l,
            _ => null
        };

    private static double? GetDoubleTag(Activity activity, string key)
        => activity.GetTagItem(key) switch
        {
            double d => d,
            float f => f,
            _ => null
        };
}
