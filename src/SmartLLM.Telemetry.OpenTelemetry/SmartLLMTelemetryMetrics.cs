using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace SmartLLM.Telemetry.OpenTelemetry;

/// <summary>OpenTelemetry metrics for LLM requests (tokens, latency, cost).</summary>
public static class SmartLLMTelemetryMetrics
{
    public const string MeterName = SmartLLMTelemetryActivitySource.Name;

    private static readonly Meter Meter = new(MeterName, "1.0.0");

    private static readonly Counter<long> Requests = Meter.CreateCounter<long>(
        "smartllm.requests",
        description: "Number of LLM or embedding requests");

    private static readonly Counter<long> EmbeddingInputs = Meter.CreateCounter<long>(
        "smartllm.embedding.inputs",
        description: "Number of embedding input texts");

    private static readonly Counter<long> Tokens = Meter.CreateCounter<long>(
        "smartllm.tokens",
        description: "Token usage by type");

    private static readonly Histogram<double> LatencyMs = Meter.CreateHistogram<double>(
        "smartllm.latency.ms",
        unit: "ms",
        description: "End-to-end LLM request latency");

    private static readonly Histogram<double> CostUsd = Meter.CreateHistogram<double>(
        "smartllm.cost.usd",
        unit: "USD",
        description: "Estimated cost per request");

    public static void RecordChatCompletion(
        string provider,
        string model,
        string status,
        int promptTokens,
        int completionTokens,
        double durationMs,
        double? estimatedCostUsd)
    {
        var baseTags = new TagList
        {
            { "provider", provider },
            { "model", model },
            { "status", status }
        };

        Requests.Add(1, baseTags);

        if (promptTokens > 0)
        {
            Tokens.Add(promptTokens, Tags(baseTags, "prompt"));
        }

        if (completionTokens > 0)
        {
            Tokens.Add(completionTokens, Tags(baseTags, "completion"));
        }

        if (durationMs >= 0)
        {
            LatencyMs.Record(durationMs, baseTags);
        }

        if (estimatedCostUsd is > 0)
        {
            CostUsd.Record(estimatedCostUsd.Value, baseTags);
        }
    }

    public static void RecordEmbedding(
        string provider,
        string model,
        string status,
        int inputCount,
        int totalTokens,
        double durationMs,
        double? estimatedCostUsd)
    {
        var baseTags = new TagList
        {
            { "provider", provider },
            { "model", model },
            { "status", status },
            { "operation", "embeddings" }
        };

        Requests.Add(1, baseTags);
        if (inputCount > 0)
        {
            EmbeddingInputs.Add(inputCount, baseTags);
        }

        if (totalTokens > 0)
        {
            Tokens.Add(totalTokens, Tags(baseTags, "prompt"));
        }

        if (durationMs >= 0)
        {
            LatencyMs.Record(durationMs, baseTags);
        }

        if (estimatedCostUsd is > 0)
        {
            CostUsd.Record(estimatedCostUsd.Value, baseTags);
        }
    }

    private static TagList Tags(TagList baseTags, string tokenType)
    {
        var tags = new TagList();
        foreach (var tag in baseTags)
        {
            tags.Add(tag);
        }

        tags.Add("token_type", tokenType);
        return tags;
    }
}
