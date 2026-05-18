# Product Roadmap

**Last updated:** 2026-05-18

## Vision

Production-grade .NET SDK for AI observability and cost control: OpenTelemetry-native, high-throughput, safe by default.

## Current position

| | |
|--|--|
| **Version** | `1.0.0` (repo; not yet tagged/published) |
| **Stage** | Phase 1–2 baseline complete |
| **Next steps** | Test → doc polish → git push → NuGet → announce |

---

## Completed milestones

| Version | Scope |
|---------|--------|
| 0.1.0 | Core, OTel traces, console sample |
| 0.2.0 | ClickHouse sink alpha, multi-provider |
| 0.3.0 | PII export, OTLP, secrets hygiene |
| **1.0.0** | Stable API, metrics, integration tests, release runbook |

---

## Phase 2 (post–1.0)

| Epic | Scope | Target |
|------|-------|--------|
| E5 Semantic cache | Vector similarity | 1.1+ |
| E4 ClickHouse | Testcontainers CI, async insert tuning | 1.1+ |

---

## Phase 3–4 (unchanged)

- E7 Failure clustering
- E8 Cost & quota management
- E9 Dashboard
- E10 Provider fallback
- E11 Prompt A/B testing

---

## Related docs

- [v1.0 backlog](v1.0-backlog.md)
- [v1.0 release runbook](../release/v1.0-release-runbook.md)
- [CHANGELOG](../../CHANGELOG.md)
