# SmartLLM.Telemetry

High-performance, OpenTelemetry-native observability and cost management SDK for .NET AI workloads.

## Features

- **Provider-agnostic interception** — Works with `Microsoft.Extensions.AI` and Semantic Kernel pipelines (OpenAI, Azure OpenAI, Ollama, and more).
- **OpenTelemetry instrumentation** — Standard `ActivitySource` spans with LLM semantic conventions (model, tokens, latency, status).
- **Offline token & cost engine** — Provider-independent token estimation and approximate cost calculation.
- **ClickHouse sink** — High-volume async batching for traces, logs, and cost events.
- **Security** — PII redaction before export.
- **Semantic cache** — Vector similarity cache (Phase 2).

## Package matrix

| Package | Description |
|---------|-------------|
| `SmartLLM.Telemetry.Core` | `ILlmClient`, interceptor pipeline, domain models |
| `SmartLLM.Telemetry.OpenTelemetry` | Activity instrumentation and exporters |
| `SmartLLM.Telemetry.Tokenizer` | Offline token counting and cost estimation |
| `SmartLLM.Telemetry.Providers.OpenAI` | OpenAI / compatible API instrumentation |
| `SmartLLM.Telemetry.Providers.AzureOpenAI` | Azure OpenAI instrumentation |
| `SmartLLM.Telemetry.Providers.Ollama` | Ollama instrumentation |
| `SmartLLM.Telemetry.Sinks.ClickHouse` | ClickHouse batch writer |
| `SmartLLM.Telemetry.Security` | PII masking interceptors |
| `SmartLLM.Telemetry.Caching.Semantic` | Semantic cache middleware (Phase 2) |

## Quick start

```bash
git clone https://github.com/erkerkan/SmartLLM.Telemetry.git
cd SmartLLM.Telemetry
dotnet restore
dotnet build
dotnet run --project samples/SmartLLM.Telemetry.Sample.Console
```

### Minimal registration

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.OpenTelemetry;
using SmartLLM.Telemetry.Providers.OpenAI;
using SmartLLM.Telemetry.Tokenizer;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddSmartLLMTelemetry(o =>
        {
            o.ServiceName = "my-ai-app";
            o.CapturePrompts = false; // recommended in production
        });
        services.AddSmartLLMTokenizer();
        services.AddConsoleTraceExporter();
        services.AddSmartLLMOpenAI();
    })
    .Build();

var client = host.Services.GetRequiredService<ILlmClient>();
```

## Documentation

- [Product roadmap](docs/product/roadmap.md)
- [System design](docs/architecture/system-design.md)
- [Package boundaries](docs/architecture/package-boundaries.md)
- [Semantic conventions](docs/telemetry/semantic-conventions.md)
- [PII redaction policy](docs/security/pii-redaction-policy.md)
- [Versioning & NuGet](docs/release/versioning-and-nuget.md)
- [Sprint 001 — Foundation](docs/sprints/sprint-001-foundation.md)

## Requirements

- .NET 8.0 SDK or later
- Optional: ClickHouse 24.x+ for sink validation

## License

MIT — see [LICENSE](LICENSE).

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).
