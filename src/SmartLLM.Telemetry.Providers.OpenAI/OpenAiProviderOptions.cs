namespace SmartLLM.Telemetry.Providers.OpenAI;

/// <summary>OpenAI provider configuration.</summary>
public sealed class OpenAiProviderOptions
{
    public const string SectionName = "SmartLLM:OpenAI";

    /// <summary>OpenAI API key. Falls back to <c>OPENAI_API_KEY</c> environment variable.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Model id (e.g. gpt-4o-mini).</summary>
    public string Model { get; set; } = "gpt-4o-mini";

    /// <summary>Optional custom endpoint for OpenAI-compatible APIs.</summary>
    public Uri? Endpoint { get; set; }

    /// <summary>When true and no API key is configured, registers a stub client for local demos.</summary>
    public bool UseStubWhenNoApiKey { get; set; } = true;
}
