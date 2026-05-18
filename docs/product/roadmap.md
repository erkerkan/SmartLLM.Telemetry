# Product Roadmap

**Last updated:** 2026-05-18

## Vision

Deliver a production-grade .NET SDK for AI observability and cost control: OpenTelemetry-native, high-throughput, and safe by default (PII-aware).

## Current position

| | |
|--|--|
| **Stage** | Phase 2 started (ClickHouse alpha + security export) |
| **Version** | `0.3.0` |
| **Proven E2E** | Multi-provider sample → OTel → ClickHouse; OTLP exporter available |
| **Next** | NuGet publish, integration tests, semantic cache (Phase 2) |

---

## Phase 1 — Foundation (MVP) — **Complete**

| Epic | Status |
|------|--------|
| E1 M.E.AI integration | Done |
| E2 OpenTelemetry instrumentation | Done |
| E3 Token counting engine | Done |

---

## Phase 2 — High Performance Storage

| Epic | Scope | Status |
|------|-------|--------|
| E4 ClickHouse sink | Batching, retry, traces/logs/costs | **Alpha done** |
| E5 Semantic cache | Vector similarity | Not started |
| E6 PII masking & security | Regex + export redaction | **v0.3 baseline done** |

### Providers

OpenAI, Azure OpenAI, Ollama, LM Studio — all with sample + docs.

---

## Phase 3–4

Unchanged — see previous epics E7–E11 in version control history.

---

## Versioning milestones

| Milestone | Status |
|-----------|--------|
| `0.1.0` | Done |
| `0.2.0` | Done (CHANGELOG) |
| `0.3.0` | **Done in repo** — tag/NuGet optional |
| `1.0.0` | Future |

---

## Related docs

- [v0.3 backlog](v0.3-backlog.md)
- [CHANGELOG](../../CHANGELOG.md)
- [API stability](../release/api-stability.md)
