using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.OpenTelemetry;
using SmartLLM.Telemetry.Providers.OpenAI;
using SmartLLM.Telemetry.Security;
using SmartLLM.Telemetry.Tokenizer;

using var listener = new ActivityListener
{
    ShouldListenTo = source => source.Name == SmartLLMTelemetryActivitySource.Name,
    Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
    ActivityStopped = activity =>
    {
        Console.WriteLine($"[activity] {activity.OperationName} status={activity.GetTagItem("smartllm.status")} " +
            $"model={activity.GetTagItem("smartllm.model_name")} tokens={activity.GetTagItem("smartllm.total_tokens")} " +
            $"cost={activity.GetTagItem("smartllm.estimated_cost_usd")}");
    }
};
ActivitySource.AddActivityListener(listener);

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSmartLLMTelemetry(o =>
        {
            o.ServiceName = "sample-console";
            o.CapturePrompts = false;
        });
        services.AddSmartLLMTokenizer();
        services.AddSmartLLMSecurity();
        services.AddConsoleTraceExporter();
        services.AddSmartLLMOpenAI();
    })
    .Build();

var client = host.Services.GetRequiredService<ILlmClient>();

var response = await client.CompleteAsync(new LlmRequest
{
    Model = "gpt-4o-mini",
    Messages =
    [
        new LlmMessage { Role = "user", Content = "Hello from SmartLLM.Telemetry sample. Contact: test@example.com" }
    ]
});

Console.WriteLine();
Console.WriteLine($"Response: {response.Content}");
Console.WriteLine($"Tokens (estimated): {response.Usage?.TotalTokens}");
Console.WriteLine($"Cost USD (estimated): {response.Usage?.EstimatedCostUsd:F6}");
