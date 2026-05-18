using System.Diagnostics;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartLLM.Telemetry.Core;
using SmartLLM.Telemetry.OpenTelemetry;
using SmartLLM.Telemetry.Providers.AzureOpenAI;
using SmartLLM.Telemetry.Providers.Ollama;
using SmartLLM.Telemetry.Providers.OpenAI;
using SmartLLM.Telemetry.Security;
using SmartLLM.Telemetry.Sinks.ClickHouse;
using SmartLLM.Telemetry.Tokenizer;

Environment.SetEnvironmentVariable("OTEL_SERVICE_NAME", "sample-console");

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

var provider = (Environment.GetEnvironmentVariable("SMARTLLM_PROVIDER") ?? "openai").Trim().ToLowerInvariant();
var clickHouseConnection = BuildClickHouseConnectionString();

var clickHouseEnabled = string.Equals(
    Environment.GetEnvironmentVariable("SMARTLLM_CLICKHOUSE_ENABLED"),
    "true",
    StringComparison.OrdinalIgnoreCase)
    || !string.IsNullOrWhiteSpace(clickHouseConnection);

var useOtlp = string.Equals(
    Environment.GetEnvironmentVariable("SMARTLLM_OTLP"),
    "true",
    StringComparison.OrdinalIgnoreCase);

var useStreaming = string.Equals(
    Environment.GetEnvironmentVariable("SMARTLLM_STREAMING"),
    "true",
    StringComparison.OrdinalIgnoreCase);

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Debug))
    .ConfigureServices(services =>
    {
        services.AddSmartLLMTelemetry(o =>
        {
            o.ServiceName = "sample-console";
            o.CapturePrompts = string.Equals(
                Environment.GetEnvironmentVariable("SMARTLLM_CAPTURE_PROMPTS"),
                "true",
                StringComparison.OrdinalIgnoreCase);
        });
        services.AddSmartLLMTokenizer();
        services.AddSmartLLMSecurity();
        if (useOtlp)
        {
            services.AddSmartLLMOtlpExporter();
            Console.WriteLine("OTLP trace exporter enabled (OTEL_EXPORTER_OTLP_ENDPOINT).");
        }
        else
        {
            services.AddConsoleTraceExporter();
        }

        RegisterProvider(services, provider);

        if (clickHouseEnabled)
        {
            services.AddSmartLLMClickHouseSink(o => o.ConnectionString = clickHouseConnection);
            Console.WriteLine($"ClickHouse sink enabled -> {clickHouseConnection}");
        }
        else
        {
            Console.WriteLine("ClickHouse sink disabled. Set SMARTLLM_CLICKHOUSE_ENABLED=true to enable.");
        }
    })
    .Build();

await host.StartAsync();

Console.WriteLine($"Provider: {provider} | Streaming: {useStreaming}");
Console.WriteLine();

var model = ResolveModel(provider);
var messages = new[]
{
    new LlmMessage { Role = "user", Content = "Hello from SmartLLM.Telemetry sample. Contact: test@example.com" }
};

if (useStreaming)
{
    var chatClient = host.Services.GetRequiredService<IChatClient>();
    var parts = new List<string>();
    await foreach (var update in chatClient.GetStreamingResponseAsync(
                       [new ChatMessage(ChatRole.User, messages[0].Content)],
                       new ChatOptions { ModelId = model }))
    {
        if (!string.IsNullOrEmpty(update.Text))
        {
            parts.Add(update.Text);
            Console.Write(update.Text);
        }
    }

    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine($"Streamed response length: {string.Concat(parts).Length} chars");
}
else
{
    var client = host.Services.GetRequiredService<ILlmClient>();
    var response = await client.CompleteAsync(new LlmRequest
    {
        Model = model,
        Messages = messages
    });

    Console.WriteLine($"Response: {response.Content}");
    Console.WriteLine($"Tokens: {response.Usage?.TotalTokens}");
    Console.WriteLine($"Cost USD (estimated): {response.Usage?.EstimatedCostUsd:F6}");
}

if (clickHouseEnabled)
{
    Console.WriteLine();
    Console.WriteLine("Waiting 5s for ClickHouse batch flush...");
    await Task.Delay(5000);
    Console.WriteLine("Verify in ClickHouse (USE smartllm_telemetry):");
    Console.WriteLine("  SELECT event_time, model_name, total_tokens, status FROM traces ORDER BY event_time DESC LIMIT 5;");
    Console.WriteLine("  SELECT event_time, severity, left(message, 80) FROM logs ORDER BY event_time DESC LIMIT 5;");
    Console.WriteLine("  SELECT event_time, model_name, total_tokens, cost_usd FROM costs ORDER BY event_time DESC LIMIT 5;");
}

await host.StopAsync();

static void RegisterProvider(IServiceCollection services, string provider)
{
    switch (provider)
    {
        case "azure":
        case "azure_openai":
            services.AddSmartLLMAzureOpenAI(o =>
            {
                o.DeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT")
                    ?? Environment.GetEnvironmentVariable("OPENAI_MODEL")
                    ?? "gpt-4o-mini";
                o.ApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
                if (Uri.TryCreate(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"), UriKind.Absolute, out var endpoint))
                {
                    o.Endpoint = endpoint;
                }

                o.UseStubWhenNoCredentials = true;
            });
            break;

        case "ollama":
            services.AddSmartLLMOllama(o =>
            {
                o.Model = Environment.GetEnvironmentVariable("OLLAMA_MODEL") ?? "llama3.2";
                if (Uri.TryCreate(Environment.GetEnvironmentVariable("OLLAMA_HOST"), UriKind.Absolute, out var endpoint))
                {
                    o.Endpoint = endpoint;
                }

                o.UseStubWhenUnavailable = true;
            });
            break;

        case "lmstudio":
        case "lm_studio":
            services.AddSmartLLMOpenAI(o =>
            {
                o.Endpoint = ResolveLmStudioEndpoint();
                o.ApiKey = Environment.GetEnvironmentVariable("LM_STUDIO_API_KEY")
                    ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                    ?? "lm-studio";
                o.Model = Environment.GetEnvironmentVariable("LM_STUDIO_MODEL")
                    ?? Environment.GetEnvironmentVariable("OPENAI_MODEL")
                    ?? "local-model";
                o.UseStubWhenNoApiKey = false;
            });
            services.AddLlmInterceptor<LmStudioTagEnrichmentInterceptor>();
            break;

        case "openai":
        default:
            if (provider is not "openai")
            {
                Console.WriteLine($"Unknown SMARTLLM_PROVIDER '{provider}', falling back to openai.");
            }

            services.AddSmartLLMOpenAI(o =>
            {
                o.Model = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o-mini";
                o.ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
                if (Uri.TryCreate(Environment.GetEnvironmentVariable("OPENAI_ENDPOINT"), UriKind.Absolute, out var endpoint))
                {
                    o.Endpoint = endpoint;
                }

                o.UseStubWhenNoApiKey = true;
            });
            break;
    }
}

static string ResolveModel(string provider)
    => provider switch
    {
        "azure" or "azure_openai" =>
            Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT")
            ?? Environment.GetEnvironmentVariable("OPENAI_MODEL")
            ?? "gpt-4o-mini",
        "ollama" =>
            Environment.GetEnvironmentVariable("OLLAMA_MODEL") ?? "llama3.2",
        "lmstudio" or "lm_studio" =>
            Environment.GetEnvironmentVariable("LM_STUDIO_MODEL")
            ?? Environment.GetEnvironmentVariable("OPENAI_MODEL")
            ?? "local-model",
        _ => Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o-mini"
    };

static Uri ResolveLmStudioEndpoint()
{
    var url = Environment.GetEnvironmentVariable("LM_STUDIO_URL")
        ?? Environment.GetEnvironmentVariable("OPENAI_ENDPOINT")
        ?? "http://localhost:1234/v1";

    return Uri.TryCreate(url, UriKind.Absolute, out var endpoint)
        ? endpoint
        : new Uri("http://localhost:1234/v1");
}

static string? BuildClickHouseConnectionString()
{
    var explicitConnection = Environment.GetEnvironmentVariable("SMARTLLM_CLICKHOUSE");
    if (!string.IsNullOrWhiteSpace(explicitConnection))
    {
        return explicitConnection.Trim();
    }

    var host = Environment.GetEnvironmentVariable("SMARTLLM_CLICKHOUSE_HOST");
    if (string.IsNullOrWhiteSpace(host))
    {
        return null;
    }

    var port = Environment.GetEnvironmentVariable("SMARTLLM_CLICKHOUSE_PORT") ?? "8123";
    var database = Environment.GetEnvironmentVariable("SMARTLLM_CLICKHOUSE_DATABASE") ?? "smartllm_telemetry";
    var username = Environment.GetEnvironmentVariable("SMARTLLM_CLICKHOUSE_USERNAME") ?? "default";
    var password = Environment.GetEnvironmentVariable("SMARTLLM_CLICKHOUSE_PASSWORD");

    var parts = new List<string>
    {
        $"Host={host.Trim()}",
        $"Port={port.Trim()}",
        $"Database={database.Trim()}",
        $"Username={username.Trim()}"
    };

    if (!string.IsNullOrEmpty(password))
    {
        parts.Add($"Password={password}");
    }

    return string.Join(';', parts);
}
