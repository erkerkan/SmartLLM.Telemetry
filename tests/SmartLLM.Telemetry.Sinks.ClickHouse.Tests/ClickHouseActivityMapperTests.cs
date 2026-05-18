using System.Diagnostics;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.OpenTelemetry;
using SmartLLM.Telemetry.Sinks.ClickHouse;
using SmartLLM.Telemetry.Security;
using Xunit;

namespace SmartLLM.Telemetry.Sinks.ClickHouse.Tests;

public sealed class ClickHouseActivityMapperTests
{
    [Fact]
    public void MapTrace_exports_extra_tags_as_attributes()
    {
        using var listener = CreateListener();
        using var activity = SmartLLMTelemetryActivitySource.Instance.StartActivity("smartllm.chat");
        activity!.SetTag(SmartLLMTelemetryActivitySource.Tags.ModelName, "gpt-4o-mini");
        activity.SetTag(SmartLLMTelemetryActivitySource.Tags.Provider, "openai");
        activity.SetTag(SmartLLMTelemetryActivitySource.Tags.TotalTokens, 42);
        activity.SetTag("custom.env", "staging");

        var row = ClickHouseActivityMapper.MapTrace(activity, new SmartLLMTelemetryOptions { ServiceName = "test-app" });

        Assert.Equal("test-app", row.ServiceName);
        Assert.Single(row.Attributes);
        Assert.Equal("staging", row.Attributes["custom.env"]);
    }

    [Fact]
    public void MapLogs_includes_prompt_event_when_capture_enabled()
    {
        using var listener = CreateListener();
        using var activity = SmartLLMTelemetryActivitySource.Instance.StartActivity("smartllm.chat");
        activity!.AddEvent(new ActivityEvent("smartllm.prompt", tags: new ActivityTagsCollection
        {
            ["content"] = "hello"
        }));

        var logs = ClickHouseActivityMapper.MapLogs(activity);

        Assert.Contains(logs, l => l.Severity == "info" && l.Message == "hello");
    }

    [Fact]
    public void MapLogs_redacts_email_when_redactor_provided()
    {
        using var listener = CreateListener();
        using var activity = SmartLLMTelemetryActivitySource.Instance.StartActivity("smartllm.chat");
        activity!.AddEvent(new ActivityEvent("smartllm.prompt", tags: new ActivityTagsCollection
        {
            ["content"] = "Contact test@example.com"
        }));

        var redactor = new PiiContentRedactor(
            new PiiRedactor(Microsoft.Extensions.Options.Options.Create(new PiiRedactionOptions())));
        var logs = ClickHouseActivityMapper.MapLogs(activity, redactor);

        var log = Assert.Single(logs);
        Assert.Contains("[REDACTED:email]", log.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("test@example.com", log.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void MapCost_returns_row_when_tokens_and_cost_present()
    {
        using var listener = CreateListener();
        using var activity = SmartLLMTelemetryActivitySource.Instance.StartActivity("smartllm.chat");
        activity!.SetTag(SmartLLMTelemetryActivitySource.Tags.TotalTokens, 10);
        activity.SetTag(SmartLLMTelemetryActivitySource.Tags.EstimatedCostUsd, 0.00012);
        activity.SetTag(SmartLLMTelemetryActivitySource.Tags.ModelName, "gpt-4o-mini");
        activity.SetTag(SmartLLMTelemetryActivitySource.Tags.ApiKeyId, "key-abc");

        var cost = ClickHouseActivityMapper.MapCost(activity);

        Assert.NotNull(cost);
        Assert.Equal(10u, cost!.TotalTokens);
        Assert.Equal(0.00012, cost.CostUsd);
        Assert.False(string.IsNullOrEmpty(cost.ApiKeyHash));
    }

    private static ActivityListener CreateListener()
    {
        var listener = new ActivityListener
        {
            ShouldListenTo = s => s.Name == SmartLLMTelemetryActivitySource.Name,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);
        return listener;
    }
}
