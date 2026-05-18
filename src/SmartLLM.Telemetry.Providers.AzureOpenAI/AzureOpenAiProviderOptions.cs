namespace SmartLLM.Telemetry.Providers.AzureOpenAI;

/// <summary>Azure OpenAI provider configuration.</summary>
public sealed class AzureOpenAiProviderOptions
{
    public const string SectionName = "SmartLLM:AzureOpenAI";

    /// <summary>Azure OpenAI resource endpoint, e.g. https://myresource.openai.azure.com/.</summary>
    public Uri? Endpoint { get; set; }

    /// <summary>API key. Falls back to <c>AZURE_OPENAI_API_KEY</c>.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Deployment name (model deployment in Azure).</summary>
    public string DeploymentName { get; set; } = "gpt-4o-mini";

    /// <summary>When true and credentials are missing, registers a stub client for local demos.</summary>
    public bool UseStubWhenNoCredentials { get; set; } = true;
}
