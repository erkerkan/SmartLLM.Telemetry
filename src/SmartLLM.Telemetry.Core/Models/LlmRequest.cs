namespace SmartLLM.Telemetry.Core;

/// <summary>LLM chat completion request.</summary>
public sealed class LlmRequest
{
    public required string Model { get; init; }

    public required IReadOnlyList<LlmMessage> Messages { get; init; }

    public string? RequestId { get; init; }

    public string? TenantId { get; init; }

    public string? ApiKeyId { get; init; }

    public double? Temperature { get; init; }

    public int? MaxTokens { get; init; }
}

/// <summary>Chat message role and content.</summary>
public sealed class LlmMessage
{
    public required string Role { get; init; }

    public required string Content { get; init; }
}
