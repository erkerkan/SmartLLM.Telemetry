using System.Diagnostics;

namespace SmartLLM.Telemetry.OpenTelemetry;

/// <summary>Central ActivitySource for LLM telemetry.</summary>
public static class SmartLLMTelemetryActivitySource
{
    public const string Name = "SmartLLM.Telemetry";

    public static readonly ActivitySource Instance = new(Name, "0.1.0");

    public static class Operations
    {
        public const string Chat = "smartllm.chat";
        public const string Embeddings = "smartllm.embeddings";
    }

    public static class Tags
    {
        public const string Provider = "smartllm.provider";
        public const string ModelName = "smartllm.model_name";
        public const string Operation = "smartllm.operation";
        public const string RequestId = "smartllm.request_id";
        public const string Status = "smartllm.status";
        public const string PromptTokens = "smartllm.prompt_tokens";
        public const string CompletionTokens = "smartllm.completion_tokens";
        public const string TotalTokens = "smartllm.total_tokens";
        public const string EstimatedCostUsd = "smartllm.estimated_cost_usd";
        public const string DurationMs = "smartllm.duration_ms";
        public const string TenantId = "smartllm.tenant_id";
    }

    public static class StatusValues
    {
        public const string Ok = "ok";
        public const string Error = "error";
        public const string Cancelled = "cancelled";
    }
}
