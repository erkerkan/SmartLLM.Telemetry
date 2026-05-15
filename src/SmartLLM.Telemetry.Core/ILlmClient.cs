namespace SmartLLM.Telemetry.Core;

/// <summary>
/// Provider-agnostic LLM client abstraction for chat completion calls.
/// </summary>
public interface ILlmClient
{
    /// <summary>Provider identifier (e.g. openai, azure_openai, ollama).</summary>
    string Provider { get; }

    /// <summary>Executes a chat completion request.</summary>
    Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken cancellationToken = default);
}
