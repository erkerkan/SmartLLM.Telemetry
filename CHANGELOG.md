# Changelog

All notable changes to this project are documented here.

## [0.3.0] - 2026-05-18

### Added

- `IContentRedactor` + PII redaction on ClickHouse log/attribute export when `AddSmartLLMSecurity()` is registered
- `AddSmartLLMOtlpExporter()` for OpenTelemetry Collector / OTLP backends
- `LmStudioTagEnrichmentInterceptor` — `smartllm.provider=lmstudio` for local LM Studio
- Model pricing aliases (deployment names, `meta-llama-*`, local models → zero cost)
- `docs/release/api-stability.md`, updated provider capability matrix
- `docker/clickhouse/.env.example` — credentials not committed; use `.env` locally

### Changed

- Sample: no hardcoded ClickHouse password; connection string from `SMARTLLM_CLICKHOUSE` only
- Documentation: provider testing, roadmap, v0.3 backlog status

### Security

- Removed default `admin`/`admin` connection string from sample code
- Docker Compose reads credentials from `.env` (optional)

## [0.2.0] - 2026-05-18

### Added

- ClickHouse sink: `traces`, `logs`, `costs`, HTTP JSONEachRow batch writer, retry on transient HTTP errors
- Providers: Azure OpenAI, Ollama (HTTP), LM Studio via OpenAI-compatible endpoint
- `InstrumentedChatClient` streaming enrichment
- Multi-provider console sample, Docker Compose for ClickHouse

## [0.1.0] - 2026-05-01

### Added

- Initial solution: Core, OpenTelemetry, Tokenizer, OpenAI provider, Security (regex PII), console sample
