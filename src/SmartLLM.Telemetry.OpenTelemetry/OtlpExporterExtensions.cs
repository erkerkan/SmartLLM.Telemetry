using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;
using SmartLLM.Telemetry.Core;

namespace SmartLLM.Telemetry.OpenTelemetry;

/// <summary>OTLP trace export for OpenTelemetry Collector / backends.</summary>
public static class OtlpExporterExtensions
{
    /// <summary>
    /// Registers SmartLLM <see cref="System.Diagnostics.ActivitySource"/> tracing with OTLP export.
    /// Set endpoint via <c>OTEL_EXPORTER_OTLP_ENDPOINT</c> or configure options.
    /// </summary>
    public static IServiceCollection AddSmartLLMOtlpExporter(
        this IServiceCollection services,
        Action<OtlpExporterOptions>? configure = null)
        => services.AddSmartLLMTracing(o =>
        {
            o.UseConsoleExporter = false;
            o.UseOtlpExporter = true;
            o.ConfigureOtlp = configure;
        });
}
