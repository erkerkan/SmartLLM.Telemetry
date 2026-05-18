# SmartLLM.Telemetry.Security

Regex-based PII redaction for ClickHouse log/attribute export paths (e.g. email patterns). Not a full DLP suite.

## Install

```bash
dotnet add package SmartLLM.Telemetry.Security
```

```csharp
services.AddSmartLLMSecurity();
```

Register before or with your telemetry pipeline. See the main repo for ClickHouse + `IContentRedactor` integration.

## Author

**Murat Erkara** — [LinkedIn](https://www.linkedin.com/in/murat-erkara) · [GitHub](https://github.com/erkerkan)

## Documentation

https://github.com/erkerkan/SmartLLM.Telemetry
