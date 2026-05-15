# Package Boundaries

## Dependency rules

```
Core                    (no telemetry SDK deps)
  ↑
Tokenizer, Security     (depends on Core)
  ↑
OpenTelemetry           (depends on Core, Tokenizer optional)
  ↑
Providers.*             (depends on Core, OpenTelemetry)
  ↑
Sinks.ClickHouse        (depends on Core, OpenTelemetry)
Caching.Semantic        (depends on Core, Sinks optional)
```

| Package | May reference | Must not reference |
|---------|---------------|-------------------|
| Core | BCL, M.E.AI abstractions | OpenTelemetry, ClickHouse |
| OpenTelemetry | Core, OTel SDK | Providers, ClickHouse |
| Tokenizer | Core | Providers |
| Providers.* | Core, OpenTelemetry | Other Providers.* |
| Sinks.ClickHouse | Core, OTel | Providers |
| Security | Core | Providers |
| Caching.Semantic | Core | Providers (direct) |

## Responsibility matrix

| Concern | Owner package |
|---------|---------------|
| `ILlmClient`, interceptors, models | Core |
| `ActivitySource`, span helpers | OpenTelemetry |
| Token count, pricing tables | Tokenizer |
| Provider-specific adapters | Providers.* |
| Batch insert, CH schema | Sinks.ClickHouse |
| PII patterns, redaction | Security |
| Embedding cache lookup | Caching.Semantic |

## Public API stability

- **Core** and **OpenTelemetry**: stabilize toward `1.0.0`.
- **Providers** and **Sinks**: may ship as `0.x` until provider matrix is complete.
- Breaking changes require ADR note in `docs/architecture/adr/` (future).
