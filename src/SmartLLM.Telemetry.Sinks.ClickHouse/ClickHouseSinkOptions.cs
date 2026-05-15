namespace SmartLLM.Telemetry.Sinks.ClickHouse;

/// <summary>ClickHouse sink configuration.</summary>
public sealed class ClickHouseSinkOptions
{
    public const string SectionName = "SmartLLM:ClickHouse";

    public string? ConnectionString { get; set; }

    public string Database { get; set; } = "smartllm_telemetry";

    public int BatchSize { get; set; } = 500;

    public TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(1);
}
