# SmartLLM.Telemetry.Sinks.ClickHouse

Batch HTTP exporter for OpenTelemetry activities to ClickHouse (`traces`, optional `logs` and `costs` tables). Includes SQL schema under `Schema/`.

## Install

```bash
dotnet add package SmartLLM.Telemetry.Sinks.ClickHouse
```

```csharp
services.AddSmartLLMClickHouse(o =>
{
    o.ConnectionString = "Host=localhost;Port=8123;Database=smartllm_telemetry;Username=...;Password=...";
});
```

Apply `Schema/001_init.sql` once. Docker compose and query examples: main repo `docker/clickhouse/`.

Requires [SmartLLM.Telemetry.OpenTelemetry](https://www.nuget.org/packages/SmartLLM.Telemetry.OpenTelemetry).

## Author

**Murat Erkara** — [LinkedIn](https://www.linkedin.com/in/murat-erkara) · [GitHub](https://github.com/erkerkan)

## Documentation

https://github.com/erkerkan/SmartLLM.Telemetry
