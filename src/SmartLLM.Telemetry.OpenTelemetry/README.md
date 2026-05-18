# SmartLLM.Telemetry.OpenTelemetry

OpenTelemetry instrumentation for SmartLLM: `ActivitySource` spans (`smartllm.chat`, `smartllm.embeddings`, `smartllm.tool`), metrics, `AddSmartLLMTracing()`, and `InstrumentedLlmClient`.

## Install

```bash
dotnet add package SmartLLM.Telemetry.OpenTelemetry
```

Requires [SmartLLM.Telemetry.Core](https://www.nuget.org/packages/SmartLLM.Telemetry.Core).

## Quick start

```csharp
services.AddSmartLLMTelemetry(o => o.ServiceName = "my-app");
services.AddSmartLLMTracing(o =>
{
    o.UseConsoleExporter = true;
    o.EnableMetrics = true;
});
```

## Documentation

https://github.com/erkerkan/SmartLLM.Telemetry
