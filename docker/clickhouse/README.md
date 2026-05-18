# ClickHouse (local dev)

## Start

```bash
cd docker/clickhouse
docker compose up -d
```

HTTP: `http://localhost:8123`

**Credentials:** copy `.env.example` to `.env` in this folder (gitignored). Default example uses `admin` / `change-me` for local dev only — never commit `.env`.

## Schema

Apply after first start (DBeaver, `clickhouse-client`, or HTTP):

`src/SmartLLM.Telemetry.Sinks.ClickHouse/Schema/001_init.sql`

## Sample app

Provider-specific commands and **expected console output** are documented in the [main README — Sample console](../../README.md#sample-console--testing-providers).

```powershell
# OpenAI (default)
$env:SMARTLLM_CLICKHOUSE = "Host=http://127.0.0.1:8123;Database=smartllm_telemetry;Username=admin;Password=change-me"
dotnet run --project samples/SmartLLM.Telemetry.Sample.Console -c Release

# Azure OpenAI
$env:SMARTLLM_PROVIDER = "azure"
$env:AZURE_OPENAI_ENDPOINT = "https://myresource.openai.azure.com/"
$env:AZURE_OPENAI_API_KEY = "..."
$env:AZURE_OPENAI_DEPLOYMENT = "gpt-4o-mini"
dotnet run --project samples/SmartLLM.Telemetry.Sample.Console -c Release

# Ollama (local)
$env:SMARTLLM_PROVIDER = "ollama"
$env:OLLAMA_HOST = "http://localhost:11434"
$env:OLLAMA_MODEL = "llama3.2"
dotnet run --project samples/SmartLLM.Telemetry.Sample.Console -c Release

# LM Studio (OpenAI-compatible local server — enable server in LM Studio UI first)
$env:SMARTLLM_PROVIDER = "lmstudio"
$env:LM_STUDIO_URL = "http://localhost:1234/v1"
$env:LM_STUDIO_MODEL = "your-model-id"
$env:SMARTLLM_CLICKHOUSE = "Host=http://127.0.0.1:8123;Database=smartllm_telemetry;Username=admin;Password=change-me"
dotnet run --project samples/SmartLLM.Telemetry.Sample.Console -c Release
```

**Success:** log line `Inserted 1 row(s) into smartllm_telemetry.traces` (see README for full checklist).

Verify:

```sql
SELECT event_time, model_name, total_tokens, status
FROM smartllm_telemetry.traces
ORDER BY event_time DESC
LIMIT 5;
```

See [docs/storage/clickhouse-schema.md](../../docs/storage/clickhouse-schema.md) for table definitions and troubleshooting.
