# Test Strategy

## Layers

| Layer | Location | Focus |
|-------|----------|-------|
| Unit | `tests/*.Tests` | Interceptors, tokenizer, redaction |
| Integration | `tests/*.IntegrationTests` (future) | ClickHouse, live providers (opt-in) |
| Sample smoke | `samples/` | Manual E2E console run |

## Required coverage (MVP)

- `LlmInterceptorPipeline` ordering and short-circuit
- `InstrumentedLlmClient` sets tags on success/failure/cancel
- `OfflineTokenCounter` estimates within tolerance vs known fixtures
- `PiiRedactor` masks email and credit card patterns

## CI gates

- `dotnet test` on windows-latest + ubuntu-latest
- Analyzers: `AnalysisLevel latest`, warnings as errors (Release)
- No flaky network tests in default CI

## Test data

- Use synthetic prompts only; no real PII in committed fixtures.
- Golden files for tokenizer under `tests/.../Fixtures/`.

## Performance tests

BenchmarkDotNet project runs in CI **smoke** mode (build only) until baselines are checked in.
