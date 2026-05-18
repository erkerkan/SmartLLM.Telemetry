# SmartLLM.Telemetry

OpenTelemetry-native **observability SDK for .NET 8 LLM chat workloads**: traces, optional metrics, estimated token/cost tags, and an optional ClickHouse sink.

## What this library does (v1.0)

- Instruments **chat completions** via `ILlmClient` or `Microsoft.Extensions.AI` `IChatClient` (including streaming).
- Emits **OpenTelemetry traces** (`smartllm.chat`) with model, tokens, latency, status, and optional estimated USD cost.
- Ships **provider packages** for **OpenAI**, **Azure OpenAI**, **Ollama**, and **LM Studio** (OpenAI-compatible local server).
- **Estimates** tokens (Tiktoken/heuristic) and cost (static pricing table) when the provider does not return usage.
- Optionally **exports** to **ClickHouse** (`traces`, and conditionally `logs` / `costs`).
- **Redacts** common PII (email, etc.) on ClickHouse log/attribute export when `AddSmartLLMSecurity()` is registered.
- Exports traces/metrics to **console or OTLP** via `AddSmartLLMTracing()`.

## What this library does not do (v1.0)

- No hosted dashboard, quota enforcement, or billing integration.
- No semantic / vector cache (package exists as a no-op placeholder).
- No dedicated Semantic Kernel package (use your existing `IChatClient` from SK with `InstrumentedChatClient`).
- No tool/function-call spans, embeddings API, or guaranteed exact token counts for every local model.
- `costs` in ClickHouse are written only when estimated cost is **> 0** (local models usually produce **no** `costs` row).

## Features

- **Chat instrumentation** — `ILlmClient` pipeline + `IChatClient` wrapper (`SmartLLM.Telemetry.Extensions.AI`).
- **Providers** — OpenAI, Azure OpenAI, Ollama (HTTP), LM Studio (via OpenAI-compatible endpoint).
- **OpenTelemetry** — `ActivitySource` spans; optional metrics (`smartllm.requests`, `smartllm.tokens`, `smartllm.latency.ms`, `smartllm.cost.usd`).
- **ClickHouse sink** — Batched HTTP JSONEachRow insert with retry (local Docker compose included).
- **Security** — Regex PII redaction on ClickHouse export paths (not a full DLP suite).
- **Stubs for demos** — OpenAI/Azure/Ollama can fall back to stub clients when credentials or Ollama are unavailable (LM Studio does not stub).

## Capability matrix (v1.0)

| Capability | Status |
|------------|--------|
| Chat `ILlmClient` + `IChatClient` tracing | Yes |
| Streaming | Yes |
| OpenAI / Azure / Ollama / LM Studio | Yes |
| ClickHouse `traces` / `logs` / `costs` | Yes |
| PII redaction on ClickHouse export | Yes (with `AddSmartLLMSecurity`) |
| OTLP + console export | Yes |
| OTel metrics (`smartllm.*`) | Yes |
| Tool/function spans | No |
| Embeddings API | No |
| Semantic cache | Placeholder only |
| Budget / quota enforcement | No |
| Built-in dashboard | No |

## Package matrix

| Package | Description |
|---------|-------------|
| `SmartLLM.Telemetry.Core` | `ILlmClient`, interceptor pipeline, domain models |
| `SmartLLM.Telemetry.OpenTelemetry` | Activity instrumentation and exporters |
| `SmartLLM.Telemetry.Tokenizer` | Offline token counting and cost estimation |
| `SmartLLM.Telemetry.Providers.OpenAI` | OpenAI / compatible API instrumentation |
| `SmartLLM.Telemetry.Providers.AzureOpenAI` | Azure OpenAI (real `IChatClient` + `ILlmClient`) |
| `SmartLLM.Telemetry.Providers.Ollama` | Ollama HTTP API + instrumented `ILlmClient` |
| `SmartLLM.Telemetry.Sinks.ClickHouse` | ClickHouse batch writer |
| `SmartLLM.Telemetry.Security` | PII masking interceptors |
| `SmartLLM.Telemetry.Caching.Semantic` | Placeholder only (no cache logic in v1.0) |
| `SmartLLM.Telemetry.Extensions.AI` | `Microsoft.Extensions.AI` `IChatClient` instrumentation |

## Quick start

```bash
git clone https://github.com/erkerkan/SmartLLM.Telemetry.git
cd SmartLLM.Telemetry
dotnet restore
dotnet build
dotnet run --project samples/SmartLLM.Telemetry.Sample.Console
```

The sample uses environment variables to pick a provider and optional ClickHouse export. See [Sample console — testing providers](#sample-console--testing-providers) below.

### Minimal registration

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.OpenTelemetry;
using SmartLLM.Telemetry.Providers.OpenAI;
using SmartLLM.Telemetry.Security;
using SmartLLM.Telemetry.Tokenizer;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddSmartLLMTelemetry(o =>
        {
            o.ServiceName = "my-ai-app";
            o.CapturePrompts = false;      // required false in production unless you need prompt logs
            o.CaptureCompletions = false;
        });
        services.AddSmartLLMTokenizer();
        services.AddSmartLLMSecurity();
        services.AddSmartLLMTracing(o =>
        {
            o.UseConsoleExporter = true;   // or o.UseOtlpExporter = true for a Collector
            o.EnableMetrics = true;
        });
        services.AddSmartLLMOpenAI();
    })
    .Build();

var client = host.Services.GetRequiredService<ILlmClient>();
```

## Sample console — testing providers

Project: `samples/SmartLLM.Telemetry.Sample.Console`

The sample sends one chat request, prints OpenTelemetry activity tags, and (when configured) flushes a batch row to ClickHouse.

### Environment variables

| Variable | Description |
|----------|-------------|
| `SMARTLLM_PROVIDER` | `openai` (default), `azure`, `ollama`, `lmstudio` |
| `SMARTLLM_STREAMING` | `true` to use streaming `IChatClient` instead of `ILlmClient` |
| `SMARTLLM_CLICKHOUSE` | Full connection string (enables sink). **Do not commit credentials.** |
| `SMARTLLM_CLICKHOUSE_HOST` | Alternative: build connection from host/port/user/password env vars |
| `SMARTLLM_OTLP` | `true` → OTLP exporter instead of console |
| `SMARTLLM_CAPTURE_PROMPTS` | `true` to capture prompt text (activity events → ClickHouse `logs` when sink enabled) |
| `OPENAI_API_KEY` / `OPENAI_MODEL` / `OPENAI_ENDPOINT` | OpenAI or compatible API |
| `AZURE_OPENAI_*` | Azure OpenAI endpoint, key, deployment |
| `OLLAMA_HOST` / `OLLAMA_MODEL` | Ollama base URL and model name |
| `LM_STUDIO_URL` / `LM_STUDIO_MODEL` / `LM_STUDIO_API_KEY` | LM Studio local OpenAI-compatible server |

On Windows, prefer `Host=http://127.0.0.1:8123` for ClickHouse (more stable than `localhost` with Docker Desktop).

### ClickHouse (optional)

```powershell
cd docker/clickhouse
docker compose up -d
# Apply schema once: src/SmartLLM.Telemetry.Sinks.ClickHouse/Schema/001_init.sql

# After: cp docker/clickhouse/.env.example docker/clickhouse/.env and set CLICKHOUSE_PASSWORD
$env:SMARTLLM_CLICKHOUSE = "Host=http://127.0.0.1:8123;Database=smartllm_telemetry;Username=admin;Password=change-me"
```

More detail: [docker/clickhouse/README.md](docker/clickhouse/README.md).

### OpenAI (cloud)

**Prerequisites:** `OPENAI_API_KEY` set.

```powershell
$env:SMARTLLM_PROVIDER = "openai"
$env:OPENAI_MODEL = "gpt-4o-mini"
$env:OPENAI_API_KEY = "sk-..."
dotnet run --project samples/SmartLLM.Telemetry.Sample.Console -c Release
```

**Without API key:** stub client runs (echo-style demo, no real API call).

**Expected (success):**

- Console: `Response: ...`, `Tokens: <n>`, `Cost USD (estimated): 0.00xxxx` (when pricing is known)
- Activity line: `[activity] smartllm.chat status=ok model=gpt-4o-mini tokens=<n> cost=<value>`
- With ClickHouse: `Inserted 1 row(s) into smartllm_telemetry.traces` and optionally a `costs` row

### Azure OpenAI

**Prerequisites:** Azure resource, deployment name, API key.

```powershell
$env:SMARTLLM_PROVIDER = "azure"
$env:AZURE_OPENAI_ENDPOINT = "https://<resource>.openai.azure.com/"
$env:AZURE_OPENAI_API_KEY = "..."
$env:AZURE_OPENAI_DEPLOYMENT = "gpt-4o-mini"
dotnet run --project samples/SmartLLM.Telemetry.Sample.Console -c Release
```

**Without credentials:** stub mode (same as OpenAI stub).

**Expected:** same shape as OpenAI; `smartllm.provider` tag is `azure_openai`.

### Ollama (local)

**Prerequisites:** [Ollama](https://ollama.com/) running; model pulled, e.g. `ollama pull llama3.2`.

```powershell
$env:SMARTLLM_PROVIDER = "ollama"
$env:OLLAMA_HOST = "http://localhost:11434"
$env:OLLAMA_MODEL = "llama3.2"
# After: cp docker/clickhouse/.env.example docker/clickhouse/.env and set CLICKHOUSE_PASSWORD
$env:SMARTLLM_CLICKHOUSE = "Host=http://127.0.0.1:8123;Database=smartllm_telemetry;Username=admin;Password=change-me"
dotnet run --project samples/SmartLLM.Telemetry.Sample.Console -c Release
```

**If Ollama is not running:** stub client (`[ollama-stub:...] echo: ...`) so you can still test telemetry wiring.

**Expected (real Ollama):**

- `Provider: ollama | Streaming: False`
- Real model reply in `Response:`
- `Tokens: <n>` (from API or tokenizer estimate)
- `Cost USD (estimated):` often empty — local models have no cloud price table
- Activity: `status=ok`, `smartllm.provider: ollama`
- ClickHouse: one row in `traces`; `costs` may be empty

### LM Studio (local, OpenAI-compatible)

**Prerequisites:** LM Studio with a loaded model and **Local Server** started (default `http://localhost:1234`).

1. In LM Studio, note the model id shown for the API (e.g. `meta-llama-3-8b-instruct`).
2. Enable the local server on port `1234`.

```powershell
$env:SMARTLLM_PROVIDER = "lmstudio"
$env:LM_STUDIO_URL = "http://localhost:1234/v1"
$env:LM_STUDIO_MODEL = "meta-llama-3-8b-instruct"
$env:LM_STUDIO_API_KEY = "lm-studio"
# After: cp docker/clickhouse/.env.example docker/clickhouse/.env and set CLICKHOUSE_PASSWORD
$env:SMARTLLM_CLICKHOUSE = "Host=http://127.0.0.1:8123;Database=smartllm_telemetry;Username=admin;Password=change-me"
dotnet run --project samples/SmartLLM.Telemetry.Sample.Console -c Release
```

LM Studio uses the OpenAI-compatible client; there is **no stub** — the server must be reachable.

**Expected (success):**

```
Provider: lmstudio | Streaming: False
ClickHouse sink enabled -> Host=http://127.0.0.1:8123;...
[activity] smartllm.chat status=ok model=meta-llama-3-8b-instruct tokens=77 cost=
Response: <model text>
Tokens: 77
Cost USD (estimated):
Inserted 1 row(s) into smartllm_telemetry.traces
```

Notes:

- `cost=` / empty **Cost USD** is normal for local models (zero-cost pricing bucket).
- Activity tag `smartllm.provider` is `lmstudio` when using the LM Studio sample path.

### Streaming

```powershell
$env:SMARTLLM_STREAMING = "true"
# plus provider env vars as above
dotnet run --project samples/SmartLLM.Telemetry.Sample.Console -c Release
```

**Expected:** tokens streamed to the console; activity `smartllm.operation` is `chat_stream` after the stream completes.

### Verify ClickHouse

```sql
USE smartllm_telemetry;

SELECT event_time, model_name, total_tokens, status
FROM traces
ORDER BY event_time DESC
LIMIT 5;

SELECT event_time, severity, left(message, 80)
FROM logs
ORDER BY event_time DESC
LIMIT 5;

SELECT event_time, model_name, total_tokens, cost_usd
FROM costs
ORDER BY event_time DESC
LIMIT 5;
```

After a successful run you should see your model name, token counts, and `status = ok` in `traces`.

- **`logs`** — only if prompt/completion capture is enabled (`SMARTLLM_CAPTURE_PROMPTS=true` and/or `CaptureCompletions=true` in code).
- **`costs`** — only when `smartllm.estimated_cost_usd` is greater than zero (typical for cloud models in the pricing table, not Ollama/LM Studio).

### OTLP (OpenTelemetry Collector)

```powershell
$env:SMARTLLM_OTLP = "true"
$env:OTEL_EXPORTER_OTLP_ENDPOINT = "http://localhost:4317"
dotnet run --project samples/SmartLLM.Telemetry.Sample.Console -c Release
```

Prefer `AddSmartLLMTracing()` (see sample) for console and/or OTLP plus metrics.

### Troubleshooting

| Symptom | Likely cause |
|---------|----------------|
| `Failed to flush ... connection forcibly closed` | Transient HTTP to ClickHouse; retry uses `127.0.0.1`, ensure container is up |
| No rows in `traces` | Schema not applied, wrong database (`USE smartllm_telemetry`), or flush failed — check logs for `Inserted N row(s)` |
| LM Studio errors | Server not started, wrong `LM_STUDIO_MODEL` id, or model not loaded |
| Ollama stub message | `ollama serve` not running or model not pulled |
| Empty `costs` | Expected for Ollama / LM Studio |

## Documentation

- [CHANGELOG](CHANGELOG.md) — version history
- [API stability](docs/release/api-stability.md) — what is stable in 1.0.x
- [Semantic conventions](docs/telemetry/semantic-conventions.md) — `smartllm.*` tags and provider matrix
- [ClickHouse schema & Docker](docker/clickhouse/README.md)
- [ClickHouse migrations](docs/storage/clickhouse-migrations.md)
- [PII redaction policy](docs/security/pii-redaction-policy.md)
- [Product roadmap](docs/product/roadmap.md)

<details>
<summary>Maintainer / release docs</summary>

- [v1.0 release runbook](docs/release/v1.0-release-runbook.md)
- [v1.0 scope](docs/product/v1.0-backlog.md)
- [Versioning & NuGet](docs/release/versioning-and-nuget.md)
- [Architecture notes](docs/architecture/system-design.md) (may describe future components)

</details>

## Requirements

- .NET 8.0 SDK or later
- Optional: ClickHouse 24.x+ for sink validation ([local Docker compose](docker/clickhouse/README.md))

## License

MIT — see [LICENSE](LICENSE).

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).
