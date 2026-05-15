# Sprint 001 — Foundation

**Goal:** Prove end-to-end LLM call tracing with offline token/cost tags and document the platform baseline.

## Stories

| ID | Story | Owner | Done when |
|----|-------|-------|-----------|
| S1 | Core + `ILlmClient` | Core | Interface + pipeline merged, tests green |
| S2 | OpenAI activity tags | Providers.OpenAI | `model_name`, `total_tokens`, latency, status on span |
| S3 | ClickHouse schema + skeleton | Sinks.ClickHouse | DDL in repo; client batches to channel |
| S4 | Console validation | Samples | Sample prints activity tags to console |

## Implementation order (Phase 1 code)

1. **Core** — Models, `ILlmClient`, `LlmInterceptorPipeline`, DI extensions.
2. **Tokenizer** — `ITokenCounter`, pricing table, cost estimator.
3. **OpenTelemetry** — `InstrumentedLlmClient`, `ActivitySource`, registration extensions.
4. **Providers.OpenAI** — Adapter + tag enrichment interceptor.
5. **Sinks.ClickHouse** — Schema SQL + `ClickHouseTelemetrySink` skeleton.
6. **Security** — Basic `PiiRedactor` + interceptor stub.
7. **Sample.Console** — Fake provider + console exporter demo.
8. **Tests** — Core + OpenTelemetry unit tests.

## Out of scope (Sprint 001)

- Live OpenAI HTTP calls in CI
- Semantic cache
- Quota blocking (Epic 8)
- Dashboard UI (Epic 9)

## Acceptance criteria

- [ ] `dotnet build` succeeds on solution
- [ ] `dotnet test` passes
- [ ] Sample run shows `smartllm.chat` activity with token tags
- [ ] Docs linked from README are present

## Risks

| Risk | Mitigation |
|------|------------|
| M.E.AI API churn | Pin package versions; adapter layer |
| Token estimate drift | Document tolerance; prefer provider usage when available |
| ClickHouse not available locally | Sink no-ops when connection string empty |
