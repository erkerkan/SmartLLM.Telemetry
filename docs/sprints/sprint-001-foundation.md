# Sprint 001 — Foundation

**Goal:** Prove end-to-end LLM call tracing with offline token/cost tags and document the platform baseline.

**Status (2026-05-18):** **Complete in code** — pending version tag `0.2.0` and formal sign-off.

## Stories

| ID | Story | Status | Done when |
|----|-------|--------|-----------|
| S1 | Core + `ILlmClient` | Done | Interface + pipeline merged, tests green |
| S2 | OpenAI activity tags | Done | `model_name`, `total_tokens`, latency, status on span |
| S3 | ClickHouse schema + sink | Done | DDL + batch writer + traces/logs/costs + retry |
| S4 | Console validation | Done | Sample + multi-provider + ClickHouse E2E |

### Also delivered (stretch)

- Azure OpenAI, Ollama HTTP client, LM Studio via OpenAI-compatible endpoint
- `InstrumentedChatClient` streaming enrichment
- ClickHouse Docker compose (named volumes), README provider testing guide
- Unit tests: Core, OTel, ClickHouse mapper/retry, OpenAI bridge, streaming

## Implementation order (Phase 1 code)

1. **Core** — Done
2. **Tokenizer** — Done (Tiktoken + `ModelPricingTable`)
3. **OpenTelemetry** — Done
4. **Providers.OpenAI** — Done (+ Azure, Ollama)
5. **Sinks.ClickHouse** — Done (alpha)
6. **Security** — Partial (regex `PiiRedactor` + interceptor)
7. **Sample.Console** — Done (real providers, not fake-only)
8. **Tests** — Done (subset of packages; integration tests deferred to v0.3)

## Out of scope (Sprint 001)

- Live OpenAI HTTP calls in CI
- Semantic cache
- Quota blocking (Epic 8)
- Dashboard UI (Epic 9)

## Acceptance criteria

- [x] `dotnet build` succeeds on solution
- [x] `dotnet test` passes
- [x] Sample run shows `smartllm.chat` activity with token tags
- [x] Docs linked from README are present
- [ ] Git release tag `v0.2.0` (release hygiene — see [v0.3-backlog.md](../product/v0.3-backlog.md))

## Risks

| Risk | Mitigation |
|------|------------|
| M.E.AI API churn | Pin package versions; adapter layer |
| Token estimate drift | Document tolerance; prefer provider usage when available |
| ClickHouse not available locally | Optional sink via `SMARTLLM_CLICKHOUSE`; Docker README |
| ClickHouse HTTP reset on Windows | Retry policy + prefer `127.0.0.1` |
