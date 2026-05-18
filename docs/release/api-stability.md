# API stability

## v1.0.0+

| Policy | Detail |
|--------|--------|
| SemVer | Breaking changes only in major versions |
| Deprecation | `[Obsolete]` for at least one minor before removal |
| Tags | `smartllm.*` attribute names are stable |

## Stable public surface

- `ILlmClient`, `LlmRequest`, `LlmResponse`, `LlmUsage`
- `SmartLLMTelemetryOptions`, `AddSmartLLMTelemetry`
- `AddInstrumentedLlmClient`, `AddSmartLLMTracing`, `AddSmartLLMOtlpExporter`, `AddConsoleTraceExporter`
- Provider extensions: `AddSmartLLMOpenAI`, `AddSmartLLMAzureOpenAI`, `AddSmartLLMOllama`
- `InstrumentedChatClient` via `AddInstrumentedChatClient`
- `IContentRedactor`, `AddSmartLLMSecurity`
- ClickHouse: `AddSmartLLMClickHouseSink`, `ClickHouseSinkOptions`, schema `001_init.sql`
- Metrics: `SmartLLMTelemetryMetrics` meter name and instrument names

## May change in minor releases

- Default stub behavior when providers are unreachable
- `ModelPricingTable` entries and alias rules
- ClickHouse batch sizing and retry defaults
- Internal mapper types

## Not part of the stable contract

- `internal` types in sink and provider factories
- Semantic cache package (Phase 2 placeholder)

## Production recommendations

1. Pin `1.0.x` package versions centrally.
2. Keep `CapturePrompts=false` unless you have a compliant log pipeline.
3. Register `AddSmartLLMSecurity()` when using ClickHouse with prompt capture.
4. Use OTLP in production; reserve console export for local dev.
