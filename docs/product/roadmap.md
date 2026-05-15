# Product Roadmap

## Vision

Deliver a production-grade .NET SDK for AI observability and cost control: OpenTelemetry-native, high-throughput, and safe by default (PII-aware).

## Phase 1 — Foundation (MVP)

| Epic | Scope | Status |
|------|-------|--------|
| E1 Semantic Kernel & M.E.AI integration | Interceptor pipeline for all M.E.AI providers | In progress |
| E2 OpenTelemetry instrumentation | ActivitySource spans, semantic tags | In progress |
| E3 Token counting engine | Offline tokenizer + approximate cost | In progress |

### Sprint 001 stories

1. `SmartLLM.Telemetry.Core` + `ILlmClient` abstraction
2. OpenAI provider activity + tags (`model_name`, `total_tokens`, latency, status)
3. ClickHouse schema + client integration skeleton
4. Console exporter E2E validation

## Phase 2 — High Performance Storage

| Epic | Scope |
|------|-------|
| E4 ClickHouse sink | Async batching, low-allocation writes |
| E5 Semantic cache | Vector similarity + ClickHouse vector search |
| E6 PII masking & security | Regex/AI redaction interceptors |

## Phase 3 — Intelligence & Analytics

| Epic | Scope |
|------|-------|
| E7 Failure clustering | Semantic grouping of errors / hallucinations |
| E8 Cost & quota management | Department/API-key budgets, hard/soft limits |
| E9 Advanced dashboard | Token heatmaps, model comparison, prompt diffs |

## Phase 4 — Reliability & Advanced Routing

| Epic | Scope |
|------|-------|
| E10 Provider fallback | Resilience policies, load balancing |
| E11 Prompt A/B testing | Side-by-side evaluation framework |

## Cross-cutting backlog (added)

| Item | Rationale |
|------|-----------|
| API stability policy | Clear v0 → v1 breaking change rules |
| Provider behavior matrix | Streaming, tools, retry, cancellation |
| Data classification levels | Never-log / mask / hash fields |
| Performance SLOs | Allocation budget, p95 overhead, batch SLA |
| Observability minimum set | Required trace + metric + log fields |
| Cache security | Key design, TTL, embedding drift |
| ClickHouse ops model | Partitioning, TTL, migrations |
| Budget policy layer | Hard/soft limits, burst, tenant isolation |

## Versioning milestones

| Milestone | Target |
|-----------|--------|
| `0.1.0` | Core + OTel + Console sample |
| `0.2.0` | ClickHouse sink (alpha) |
| `0.3.0` | PII security package |
| `1.0.0` | Stable public API, provider matrix complete |
