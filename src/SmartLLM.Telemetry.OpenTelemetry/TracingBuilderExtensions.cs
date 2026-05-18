using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace SmartLLM.Telemetry.OpenTelemetry;

/// <summary>Unified trace/metric export registration.</summary>
public static class TracingBuilderExtensions
{
    /// <summary>Configures SmartLLM trace and metric export.</summary>
    public static IServiceCollection AddSmartLLMTracing(
        this IServiceCollection services,
        Action<SmartLLMTracingOptions>? configure = null)
    {
        var options = new SmartLLMTracingOptions();
        configure?.Invoke(options);

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                Microsoft.Extensions.Options.IConfigureOptions<global::OpenTelemetry.Resources.ResourceBuilder>,
                SmartLLMResourceConfiguration>());

        var otel = services.AddOpenTelemetry();

        otel.WithTracing(builder =>
        {
            builder.AddSource(SmartLLMTelemetryActivitySource.Name);
            if (options.UseConsoleExporter)
            {
                builder.AddConsoleExporter();
            }

            if (options.UseOtlpExporter)
            {
                if (options.ConfigureOtlp is null)
                {
                    builder.AddOtlpExporter();
                }
                else
                {
                    builder.AddOtlpExporter(options.ConfigureOtlp);
                }
            }
        });

        if (options.EnableMetrics)
        {
            otel.WithMetrics(builder =>
            {
                builder.AddMeter(SmartLLMTelemetryMetrics.MeterName);
                if (options.UseConsoleExporter)
                {
                    builder.AddConsoleExporter();
                }

                if (options.UseOtlpExporter)
                {
                    if (options.ConfigureOtlp is null)
                    {
                        builder.AddOtlpExporter();
                    }
                    else
                    {
                        builder.AddOtlpExporter(options.ConfigureOtlp);
                    }
                }
            });
        }

        return services;
    }
}

/// <summary>Trace and metric export options for <see cref="TracingBuilderExtensions.AddSmartLLMTracing"/>.</summary>
public sealed class SmartLLMTracingOptions
{
    public bool UseConsoleExporter { get; set; } = true;

    public bool UseOtlpExporter { get; set; }

    public bool EnableMetrics { get; set; } = true;

    public Action<OtlpExporterOptions>? ConfigureOtlp { get; set; }
}
