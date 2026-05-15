# Performance Principles

## Targets (Phase 1 MVP)

| Metric | Target |
|--------|--------|
| Interceptor overhead (p95) | &lt; 2 ms excluding provider RTT |
| Allocations per call (hot path) | &lt; 4 KB (non-streaming) |
| ClickHouse batch flush | Configurable; default 1s or 500 rows |
| Sink backpressure | Drop-oldest or block (configurable) |

## Guidelines

### Interceptors

- Prefer `ReadOnlySpan<char>` and `ArrayPool` for large prompt buffers.
- Avoid LINQ and boxing in pre/post-call hooks.
- Use `ValueTask` when no async I/O in interceptor.

### Telemetry

- Tag cardinality: do not put raw prompts in span attributes by default.
- Use events (`Activity.AddEvent`) for large payloads when `CapturePrompts=true`.

### ClickHouse sink

- Buffer rows in `Channel<T>` with bounded capacity.
- Serialize with `Utf8JsonWriter` over pooled buffers.
- Single background flush loop per sink instance.

### Benchmarks

Run `benchmarks/SmartLLM.Telemetry.Benchmarks` in CI smoke mode (non-gating until baseline established).

## Profiling checklist

- [ ] dotnet-counters: GC heap, allocation rate
- [ ] dotnet-trace: SmartLLM.Telemetry activities
- [ ] Compare with/without interceptors enabled
