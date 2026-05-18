# SmartLLM.Telemetry.Providers.OpenAI

OpenAI and OpenAI-compatible API support: instrumented `ILlmClient`, `IChatClient`, and optional `IEmbeddingClient` (`AddSmartLLMOpenAIEmbeddings()`).

## Install

```bash
dotnet add package SmartLLM.Telemetry.Providers.OpenAI
```

```csharp
services.AddSmartLLMOpenAI(o =>
{
    o.Model = "gpt-4o-mini";
    o.ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
});
```

Pair with [SmartLLM.Telemetry.OpenTelemetry](https://www.nuget.org/packages/SmartLLM.Telemetry.OpenTelemetry). LM Studio works via a compatible endpoint + interceptor (see repo sample).

## Documentation

https://github.com/erkerkan/SmartLLM.Telemetry
