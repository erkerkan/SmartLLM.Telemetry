# Contributing to SmartLLM.Telemetry

Thank you for contributing. This project follows a documentation-first workflow.

## Development setup

1. Install [.NET 8 SDK](https://dotnet.microsoft.com/download) or later.
2. Clone the repository and run `dotnet restore && dotnet build`.
3. Run tests: `dotnet test`.
4. Run the sample: `dotnet run --project samples/SmartLLM.Telemetry.Sample.Console`.

## Pull request guidelines

- Keep changes scoped to a single epic or story when possible.
- Update relevant docs under `docs/` when changing public APIs or telemetry semantics.
- Add or update unit tests for Core, OpenTelemetry, and Tokenizer changes.
- Follow existing naming: `SmartLLM.Telemetry.*` packages and `smartllm.*` activity tags.
- Do not log raw prompts in tests or samples unless `CapturePrompts` is explicitly enabled.

## Code style

- Enable nullable reference types; avoid suppressions without justification.
- Prefer `ValueTask` for hot paths in interceptors when allocation-sensitive.
- Use `ActivitySource` for tracing; do not introduce parallel tracing APIs.

## Commit messages

Use conventional commits where practical:

- `feat(core): add quota interceptor`
- `fix(otel): set status on cancellation`
- `docs: update semantic conventions`

## Release process

Maintainers tag `v*.*.*` to trigger the release workflow. See [docs/release/versioning-and-nuget.md](docs/release/versioning-and-nuget.md).
