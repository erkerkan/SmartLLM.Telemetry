using Microsoft.Extensions.AI;
using SmartLLM.Telemetry.Core;

namespace SmartLLM.Telemetry.Extensions.AI;

/// <summary>Maps between <see cref="ILlmClient"/> models and <see cref="IChatClient"/> messages.</summary>
public static class ChatClientLlmBridge
{
    public static List<ChatMessage> ToChatMessages(IReadOnlyList<LlmMessage> messages)
    {
        var result = new List<ChatMessage>(messages.Count);
        foreach (var message in messages)
        {
            result.Add(new ChatMessage(ToChatRole(message.Role), message.Content));
        }

        return result;
    }

    public static ChatOptions ToChatOptions(LlmRequest request)
        => new()
        {
            ModelId = request.Model,
            Temperature = request.Temperature.HasValue ? (float)request.Temperature.Value : null,
            MaxOutputTokens = request.MaxTokens
        };

    public static LlmResponse ToLlmResponse(ChatResponse response, string model, TimeSpan duration)
    {
        var usage = response.Usage;
        LlmUsage? llmUsage = null;
        if (usage is not null && (usage.InputTokenCount > 0 || usage.OutputTokenCount > 0))
        {
            llmUsage = new LlmUsage
            {
                PromptTokens = (int)(usage.InputTokenCount ?? 0),
                CompletionTokens = (int)(usage.OutputTokenCount ?? 0),
                IsEstimated = false
            };
        }

        return new LlmResponse
        {
            Content = response.Text ?? string.Empty,
            Model = model,
            FinishReason = response.FinishReason?.ToString(),
            Duration = duration,
            Usage = llmUsage
        };
    }

    public static ChatResponse ToChatResponse(IReadOnlyList<ChatMessage> messages, string completionText, UsageDetails? usage)
        => new(new ChatMessage(ChatRole.Assistant, completionText))
        {
            Usage = usage
        };

    private static ChatRole ToChatRole(string role)
        => role.ToLowerInvariant() switch
        {
            "system" => ChatRole.System,
            "assistant" => ChatRole.Assistant,
            "tool" => ChatRole.Tool,
            _ => ChatRole.User
        };
}
