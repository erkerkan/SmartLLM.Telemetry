using System.Diagnostics.Metrics;
using SmartLLM.Telemetry.OpenTelemetry;
using Xunit;

namespace SmartLLM.Telemetry.OpenTelemetry.Tests;

public sealed class SmartLLMTelemetryMetricsTests
{
    [Fact]
    public void RecordChatCompletion_emits_measurements()
    {
        long requests = 0;
        long tokens = 0;

        using var listener = new MeterListener
        {
            InstrumentPublished = (instrument, meterListener) =>
            {
                if (instrument.Meter.Name == SmartLLMTelemetryMetrics.MeterName)
                {
                    meterListener.EnableMeasurementEvents(instrument);
                }
            }
        };

        listener.SetMeasurementEventCallback<long>((instrument, measurement, _, _) =>
        {
            if (instrument.Name == "smartllm.requests")
            {
                requests += measurement;
            }
            else if (instrument.Name == "smartllm.tokens")
            {
                tokens += measurement;
            }
        });

        listener.Start();

        SmartLLMTelemetryMetrics.RecordChatCompletion(
            "openai",
            "gpt-4o-mini",
            "ok",
            promptTokens: 5,
            completionTokens: 10,
            durationMs: 120,
            estimatedCostUsd: 0.0001);

        Assert.Equal(1, requests);
        Assert.Equal(15, tokens);
    }
}
