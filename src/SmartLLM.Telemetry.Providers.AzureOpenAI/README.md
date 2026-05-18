# SmartLLM.Telemetry.Providers.AzureOpenAI

Azure OpenAI instrumentation: `IChatClient` and `ILlmClient` with deployment name, endpoint, and API key configuration.

## Install

```bash
dotnet add package SmartLLM.Telemetry.Providers.AzureOpenAI
```

```csharp
services.AddSmartLLMAzureOpenAI(o =>
{
    o.DeploymentName = "gpt-4o-mini";
    o.Endpoint = new Uri("https://your-resource.openai.azure.com/");
    o.ApiKey = "...";
});
```

Requires [SmartLLM.Telemetry.OpenTelemetry](https://www.nuget.org/packages/SmartLLM.Telemetry.OpenTelemetry).

## Documentation

https://github.com/erkerkan/SmartLLM.Telemetry
