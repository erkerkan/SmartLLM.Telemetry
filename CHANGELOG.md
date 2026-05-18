# Changelog

All notable changes to this project are documented here.

## [1.2.1] - 2026-05-18

### Changed

- NuGet package README for all published packages (`PackageReadmeFile`)
- Package metadata: author **Murat Erkara** (NuGet `Authors`, LICENSE, repo and package READMEs)

## [1.2.0] - 2026-05-18

### Added (v1.1 scope)

- Tool/function-call telemetry: child spans `smartllm.tool`, events `smartllm.tool_call` / `smartllm.tool_result`
- Tags `smartllm.tool_call_count`, `smartllm.tool_result_count`, operation `chat_with_tools`
- ClickHouse `ExportZeroCostRows` option and expanded embedding model pricing
- ClickHouse query pack: `docker/clickhouse/queries/README.md`

### Added (v1.2 scope)

- `IEmbeddingClient`, `InstrumentedEmbeddingClient`, `AddSmartLLMOpenAIEmbeddings()`
- `InstrumentedEmbeddingGenerator` for `IEmbeddingGenerator<string, Embedding>`
- Embedding metrics (`smartllm.embedding.inputs`)
- Sample: `SMARTLLM_EMBED=true` smoke path

## [1.0.0] - 2026-05-18

### Added

- `AddSmartLLMTracing()` — unified console and/or OTLP trace + metric export
- OpenTelemetry metrics: `smartllm.requests`, `smartllm.tokens`, `smartllm.latency.ms`, `smartllm.cost.usd`
- ClickHouse batch writer HTTP integration test
- ClickHouse schema migration guide
- v1.0 release runbook and LinkedIn announcement draft

### Changed

- `ClickHouseActivityExporter` enqueues asynchronously (no sync blocking on activity stop)
- `ActivitySource` version `1.0.0`
- Package version line `1.0.0` — stable public API per `docs/release/api-stability.md`

### Fixed

- OpenTelemetry packages at 1.15.3 (OTLP security advisory)

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
