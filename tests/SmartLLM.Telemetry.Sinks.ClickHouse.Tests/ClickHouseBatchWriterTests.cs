using System.Net;
using System.Text;
using SmartLLM.Telemetry.Sinks.ClickHouse;
using Xunit;

namespace SmartLLM.Telemetry.Sinks.ClickHouse.Tests;

public sealed class ClickHouseBatchWriterTests
{
    [Fact]
    public async Task WriteTracesAsync_posts_json_each_row_to_traces_table()
    {
        HttpRequestMessage? captured = null;
        string? capturedBody = null;
        var handler = new StubHttpHandler(async req =>
        {
            captured = req;
            capturedBody = req.Content is null
                ? null
                : await req.Content.ReadAsStringAsync().ConfigureAwait(false);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var settings = new ClickHouseConnectionSettings
        {
            Host = "127.0.0.1",
            Port = 8123,
            Database = "smartllm_telemetry",
            Username = "test",
            Password = "secret"
        };

        using var client = new HttpClient(handler);
        using var writer = new ClickHouseBatchWriter(settings, client, logger: null, ownsHttpClient: false);

        await writer.WriteTracesAsync([
            new TraceRow
            {
                EventTime = DateTime.UtcNow,
                TraceId = "abc",
                SpanId = "def",
                ServiceName = "unit-test",
                Operation = "chat",
                Provider = "openai",
                ModelName = "gpt-4o-mini",
                Status = "ok",
                TotalTokens = 10
            }
        ]);

        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Post, captured!.Method);
        Assert.Contains("INSERT%20INTO%20traces", captured.RequestUri!.Query, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("test", captured.Headers.GetValues("X-ClickHouse-User").Single());

        Assert.Contains("\"trace_id\":\"abc\"", capturedBody!, StringComparison.Ordinal);
        Assert.Contains("\"model_name\":\"gpt-4o-mini\"", capturedBody!, StringComparison.Ordinal);
    }

    private sealed class StubHttpHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

        public StubHttpHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler) => _handler = handler;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => _handler(request);
    }
}
