namespace SmartLLM.Telemetry.Core;

/// <summary>Global SmartLLM telemetry configuration.</summary>
public sealed class SmartLLMTelemetryOptions
{
    public const string SectionName = "SmartLLM:Telemetry";

    public string ServiceName { get; set; } = "smartllm-app";

    public bool CapturePrompts { get; set; }

    public bool CaptureCompletions { get; set; }

    public string? DefaultTenantId { get; set; }
}
