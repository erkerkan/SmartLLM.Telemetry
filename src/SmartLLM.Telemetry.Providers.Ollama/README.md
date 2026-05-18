# SmartLLM.Telemetry.Providers.Ollama

Ollama HTTP API (`/api/chat`) with an instrumented `ILlmClient` and streaming support.

## Install

```bash
dotnet add package SmartLLM.Telemetry.Providers.Ollama
```

```csharp
services.AddSmartLLMOllama(o =>
{
    o.Endpoint = new Uri("http://localhost:11434");
    o.Model = "llama3.2";
});
```

Requires [SmartLLM.Telemetry.OpenTelemetry](https://www.nuget.org/packages/SmartLLM.Telemetry.OpenTelemetry). Token usage may be estimated when Ollama does not return counts.

## Documentation

https://github.com/erkerkan/SmartLLM.Telemetry
