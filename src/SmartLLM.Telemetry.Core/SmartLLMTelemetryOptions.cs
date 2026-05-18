namespace SmartLLM.Telemetry.Core;

/// <summary>Global SmartLLM telemetry configuration.</summary>
public sealed class SmartLLMTelemetryOptions
{
    public const string SectionName = "SmartLLM:Telemetry";

    public string ServiceName { get; set; } = "smartllm-app";

    public bool CapturePrompts { get; set; }

    public bool CaptureCompletions { get; set; }

    /// <summary>When true, include tool argument payloads in activity events (not recommended in production).</summary>
    public bool CaptureToolArguments { get; set; }

    /// <summary>When true, include tool result payloads in activity events.</summary>
    public bool CaptureToolResults { get; set; }

    public string? DefaultTenantId { get; set; }
}
