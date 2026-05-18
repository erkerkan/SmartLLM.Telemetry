# API stability (pre-1.0)

| Version range | Policy |
|---------------|--------|
| `0.x` | Breaking changes allowed in minor releases; documented in `CHANGELOG.md` |
| `1.0+` | SemVer: breaking changes only in major; `[Obsolete]` one minor before removal |

## Stable enough for production pilots

- `ILlmClient`, `SmartLLMTelemetryOptions`, Activity tag names (`smartllm.*`)
- `AddSmartLLMTelemetry`, `AddInstrumentedLlmClient`, provider `AddSmartLLM*` extensions
- ClickHouse sink public options and schema `001_init.sql`

## May change without a major bump before 1.0

- ClickHouse row shapes and batch internals
- Stub provider defaults
- Pricing table entries and model alias resolution
- Optional interceptors and export redaction hooks

## Recommendations for adopters

1. Pin exact package version in `Directory.Packages.props` or `PackageReference`.
2. Keep `CapturePrompts=false` in production unless you operate a compliant log pipeline.
3. Do not rely on undocumented `internal` APIs.
