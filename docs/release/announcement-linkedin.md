# LinkedIn announcement — v1.2.0

> NuGet live. Create a [GitHub Release](https://github.com/erkerkan/SmartLLM.Telemetry/releases/new) for tag `v1.2.0` so the repo shows a release (NuGet.org packages do not appear in GitHub’s “Packages” sidebar).

---

## Full post (English)

**SmartLLM.Telemetry 1.2** is on NuGet — OpenTelemetry-native observability for .NET 8 LLM apps.

What’s new in 1.2:
- **Tool / function-call telemetry** — child spans `smartllm.tool`, events for tool calls and results
- **Embeddings** — `IEmbeddingClient`, OpenAI provider, `IEmbeddingGenerator` wrapper
- **ClickHouse** query pack + optional zero-cost row export
- **10 modular packages** — pick Core + OpenTelemetry + your provider(s)

What you get:
- Chat tracing (`smartllm.chat`) with model, tokens, latency, estimated cost
- Providers: **OpenAI**, **Azure OpenAI**, **Ollama**, **LM Studio**
- Optional **ClickHouse** sink, **PII redaction**, **OTLP** + metrics

```bash
dotnet add package SmartLLM.Telemetry.Core --version 1.2.0
dotnet add package SmartLLM.Telemetry.OpenTelemetry --version 1.2.0
dotnet add package SmartLLM.Telemetry.Providers.OpenAI --version 1.2.0
```

GitHub: https://github.com/erkerkan/SmartLLM.Telemetry  
NuGet: https://www.nuget.org/packages/SmartLLM.Telemetry.Core

By **Murat Erkara** — https://www.linkedin.com/in/murat-erkara

Feedback and contributions welcome.

#dotnet #opentelemetry #llm #observability #clickhouse #ai #csharp

---

## Full post (Türkçe)

**.NET 8** için **SmartLLM.Telemetry 1.2** NuGet’te yayında — LLM uygulamalarına OpenTelemetry tabanlı gözlemlenebilirlik.

1.2 ile gelenler:
- **Tool / function-call** telemetrisi (`smartllm.tool` span’leri)
- **Embedding** API’si ve OpenAI provider
- **ClickHouse** sorgu paketi, modüler **10 paket**

OpenAI, Azure OpenAI, Ollama, LM Studio; isteğe bağlı ClickHouse, PII maskeleme, OTLP + metrikler.

```bash
dotnet add package SmartLLM.Telemetry.Core --version 1.2.0
dotnet add package SmartLLM.Telemetry.OpenTelemetry --version 1.2.0
dotnet add package SmartLLM.Telemetry.Providers.OpenAI --version 1.2.0
```

GitHub: https://github.com/erkerkan/SmartLLM.Telemetry  
NuGet: https://www.nuget.org/packages/SmartLLM.Telemetry.Core

**Murat Erkara** — https://www.linkedin.com/in/murat-erkara

Geri bildirim ve katkıya açığız.

#dotnet #opentelemetry #llm #observability #csharp #yazilim

---

## Short variant (English)

**SmartLLM.Telemetry 1.2** — .NET LLM observability on NuGet: OTel traces, tool spans, embeddings, ClickHouse sink, OpenAI/Azure/Ollama/LM Studio.

https://github.com/erkerkan/SmartLLM.Telemetry  
https://www.nuget.org/packages/SmartLLM.Telemetry.Core

#dotnet #opentelemetry #llm
