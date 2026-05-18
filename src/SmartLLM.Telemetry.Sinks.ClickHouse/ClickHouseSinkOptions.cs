namespace SmartLLM.Telemetry.Sinks.ClickHouse;

/// <summary>ClickHouse sink configuration.</summary>
public sealed class ClickHouseSinkOptions
{
    public const string SectionName = "SmartLLM:ClickHouse";

    public string? ConnectionString { get; set; }

    public string Database { get; set; } = "smartllm_telemetry";

    public int BatchSize { get; set; } = 500;

    public TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>Maximum insert attempts per batch (including the first try).</summary>
    /// <summary>When true and <see cref="Core.IContentRedactor"/> is registered, redact log messages and custom attributes.</summary>
    public bool RedactExportedContent { get; set; } = true;

    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>Initial delay before the first retry.</summary>
    public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromMilliseconds(200);
}
