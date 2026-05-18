namespace SmartLLM.Telemetry.Providers.Ollama;

/// <summary>Ollama provider configuration.</summary>
public sealed class OllamaProviderOptions
{
    public const string SectionName = "SmartLLM:Ollama";

    /// <summary>Ollama API base URL. Falls back to <c>OLLAMA_HOST</c> or http://localhost:11434.</summary>
    public Uri Endpoint { get; set; } = new("http://localhost:11434");

    /// <summary>Default model name (e.g. llama3.2, phi3).</summary>
    public string Model { get; set; } = "llama3.2";

    /// <summary>When true and Ollama is unreachable at startup, use a stub client for demos.</summary>
    public bool UseStubWhenUnavailable { get; set; } = true;
}
