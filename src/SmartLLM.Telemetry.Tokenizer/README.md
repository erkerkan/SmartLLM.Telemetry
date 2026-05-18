# SmartLLM.Telemetry.Tokenizer

Offline token counting (Tiktoken/heuristic) and estimated USD cost via a static pricing table when the provider does not return usage.

## Install

```bash
dotnet add package SmartLLM.Telemetry.Tokenizer
```

```csharp
services.AddSmartLLMTokenizer();
```

Use with [SmartLLM.Telemetry.Core](https://www.nuget.org/packages/SmartLLM.Telemetry.Core) and a provider package.

## Documentation

https://github.com/erkerkan/SmartLLM.Telemetry
