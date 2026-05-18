using Microsoft.Extensions.AI;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.Extensions.AI;
using Xunit;

namespace SmartLLM.Telemetry.Extensions.AI.Tests;

public sealed class ToolCallTelemetryTests
{
    [Fact]
    public void CollectFunctionCalls_reads_function_call_content()
    {
        var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, [
            new FunctionCallContent("call-1", "get_weather", new Dictionary<string, object?> { ["city"] = "Istanbul" })
        ]));

        var calls = ToolCallTelemetry.CollectFunctionCalls(response);

        var call = Assert.Single(calls);
        Assert.Equal("get_weather", call.Name);
        Assert.Equal("call-1", call.CallId);
    }

    [Fact]
    public void CollectToolResults_reads_tool_role_messages()
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.Tool, [new FunctionResultContent("call-1", "sunny")])
        };

        var results = ToolCallTelemetry.CollectToolResults(messages);

        var result = Assert.Single(results);
        Assert.Equal("call-1", result.CallId);
        Assert.Contains("sunny", result.Content, StringComparison.Ordinal);
    }
}
