using System.Diagnostics;
using Microsoft.Extensions.AI;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.OpenTelemetry;

namespace SmartLLM.Telemetry.Extensions.AI;

/// <summary>Records tool/function-call telemetry on chat activities.</summary>
internal static class ToolCallTelemetry
{
    public static void Record(
        Activity? parent,
        IReadOnlyList<ChatMessage> requestMessages,
        ChatResponse response,
        SmartLLMTelemetryOptions options)
    {
        if (parent is null)
        {
            return;
        }

        var calls = CollectFunctionCalls(response);
        var results = CollectToolResults(requestMessages);

        if (calls.Count == 0 && results.Count == 0)
        {
            return;
        }

        parent.SetTag(SmartLLMTelemetryActivitySource.Tags.ToolCallCount, calls.Count);
        parent.SetTag(SmartLLMTelemetryActivitySource.Tags.ToolResultCount, results.Count);

        if (calls.Count > 0)
        {
            var operation = parent.GetTagItem(SmartLLMTelemetryActivitySource.Tags.Operation)?.ToString();
            if (operation is "chat" or "chat_stream")
            {
                parent.SetTag(SmartLLMTelemetryActivitySource.Tags.Operation, "chat_with_tools");
            }
        }

        foreach (var call in calls)
        {
            RecordFunctionCall(parent, call, options);
        }

        foreach (var result in results)
        {
            RecordToolResultEvent(parent, result, options);
        }
    }

    private static void RecordFunctionCall(Activity parent, FunctionCallInfo call, SmartLLMTelemetryOptions options)
    {
        using var child = SmartLLMTelemetryActivitySource.Instance.StartActivity(
            SmartLLMTelemetryActivitySource.Operations.Tool,
            ActivityKind.Internal,
            parent.Context);

        child?.SetTag(SmartLLMTelemetryActivitySource.Tags.Provider,
            parent.GetTagItem(SmartLLMTelemetryActivitySource.Tags.Provider));
        child?.SetTag(SmartLLMTelemetryActivitySource.Tags.ModelName,
            parent.GetTagItem(SmartLLMTelemetryActivitySource.Tags.ModelName));
        child?.SetTag(SmartLLMTelemetryActivitySource.Tags.Operation, "tool_call");
        child?.SetTag(SmartLLMTelemetryActivitySource.Tags.ToolName, call.Name);
        child?.SetTag(SmartLLMTelemetryActivitySource.Tags.ToolCallId, call.CallId);
        child?.SetStatus(ActivityStatusCode.Ok);

        var args = options.CaptureToolArguments ? call.Arguments : null;
        parent.AddEvent(new ActivityEvent("smartllm.tool_call", tags: new ActivityTagsCollection
        {
            ["tool.name"] = call.Name,
            ["tool.call_id"] = call.CallId,
            ["tool.arguments"] = args ?? string.Empty
        }));
    }

    private static void RecordToolResultEvent(Activity parent, ToolResultInfo result, SmartLLMTelemetryOptions options)
    {
        var content = options.CaptureToolResults ? result.Content : null;
        parent.AddEvent(new ActivityEvent("smartllm.tool_result", tags: new ActivityTagsCollection
        {
            ["tool.call_id"] = result.CallId,
            ["tool.result"] = content ?? string.Empty
        }));
    }

    internal static List<FunctionCallInfo> CollectFunctionCalls(ChatResponse response)
    {
        var list = new List<FunctionCallInfo>();
        foreach (var message in response.Messages)
        {
            if (message.Contents is null)
            {
                continue;
            }

            foreach (var content in message.Contents)
            {
                TryAddFunctionCall(content, list);
            }
        }

        return list;
    }

    internal static List<ToolResultInfo> CollectToolResults(IReadOnlyList<ChatMessage> messages)
    {
        var list = new List<ToolResultInfo>();
        foreach (var message in messages)
        {
            if (message.Role != ChatRole.Tool)
            {
                continue;
            }

            var callId = message.MessageId ?? string.Empty;
            var text = message.Text ?? string.Empty;
            if (message.Contents is not null)
            {
                foreach (var content in message.Contents)
                {
                    if (content is FunctionResultContent result)
                    {
                        callId = result.CallId ?? callId;
                        text = result.Result?.ToString() ?? text;
                    }
                }
            }

            list.Add(new ToolResultInfo(callId, text));
        }

        return list;
    }

    private static void TryAddFunctionCall(AIContent content, List<FunctionCallInfo> list)
    {
        if (content is not FunctionCallContent call)
        {
            return;
        }

        list.Add(new FunctionCallInfo(
            call.Name ?? "unknown",
            call.CallId ?? string.Empty,
            call.Arguments?.ToString() ?? string.Empty));
    }

    internal readonly record struct FunctionCallInfo(string Name, string CallId, string Arguments);

    internal readonly record struct ToolResultInfo(string CallId, string Content);
}
