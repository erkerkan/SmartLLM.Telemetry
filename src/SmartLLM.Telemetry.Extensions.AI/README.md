# SmartLLM.Telemetry.Extensions.AI

Instrumentation for `Microsoft.Extensions.AI` `IChatClient`: wraps your client with spans, metrics, and tool/function-call telemetry (`smartllm.tool`).

## Install

```bash
dotnet add package SmartLLM.Telemetry.Extensions.AI
```

Requires [SmartLLM.Telemetry.OpenTelemetry](https://www.nuget.org/packages/SmartLLM.Telemetry.OpenTelemetry) and a provider (e.g. OpenAI).

```csharp
services.AddInstrumentedChatClient<YourChatClient>();
// or InstrumentedEmbeddingGenerator for IEmbeddingGenerator
```

## Author

**Murat Erkara** — [LinkedIn](https://www.linkedin.com/in/murat-erkara) · [GitHub](https://github.com/erkerkan)

## Documentation

https://github.com/erkerkan/SmartLLM.Telemetry
