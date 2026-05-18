# LinkedIn announcement draft (v1.0)

> Edit placeholders before posting. Do not publish until NuGet packages are live.

---

**SmartLLM.Telemetry 1.0** is on NuGet — OpenTelemetry-native observability for .NET AI apps.

What you get:
- LLM chat tracing (`smartllm.*` spans) with model, tokens, latency, and estimated cost
- Providers: **OpenAI**, **Azure OpenAI**, **Ollama**, **LM Studio**
- **ClickHouse** sink for traces, logs, and cost analytics
- **PII redaction** on export paths
- **OTLP** + metrics for your existing Grafana / Jaeger / Collector stack

Built on **.NET 8**, **Microsoft.Extensions.AI**, and **OpenTelemetry**.

```bash
dotnet add package SmartLLM.Telemetry.Core
dotnet add package SmartLLM.Telemetry.OpenTelemetry
dotnet add package SmartLLM.Telemetry.Providers.OpenAI
```

Repo: https://github.com/erkerkan/SmartLLM.Telemetry

Feedback and contributions welcome.

#dotnet #opentelemetry #llm #observability #clickhouse #ai

---

## Short variant

**SmartLLM.Telemetry 1.0** — .NET SDK for LLM observability: OTel traces + metrics, ClickHouse sink, OpenAI/Azure/Ollama/LM Studio. Now on NuGet.

https://github.com/erkerkan/SmartLLM.Telemetry
