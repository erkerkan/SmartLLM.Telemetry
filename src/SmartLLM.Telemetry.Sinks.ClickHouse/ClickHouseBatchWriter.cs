using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace SmartLLM.Telemetry.Sinks.ClickHouse;

/// <summary>HTTP JSONEachRow writer for traces, logs, and costs tables.</summary>
public sealed class ClickHouseBatchWriter : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly ClickHouseConnectionSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ClickHouseBatchWriter>? _logger;
    private readonly bool _ownsHttpClient;

    public ClickHouseBatchWriter(ClickHouseConnectionSettings settings, ILogger<ClickHouseBatchWriter>? logger = null)
        : this(settings, CreateHttpClient(), logger, ownsHttpClient: true)
    {
    }

    internal static HttpClient CreateHttpClient()
    {
        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            ConnectTimeout = TimeSpan.FromSeconds(10)
        };

        return new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(60)
        };
    }

    internal ClickHouseBatchWriter(
        ClickHouseConnectionSettings settings,
        HttpClient httpClient,
        ILogger<ClickHouseBatchWriter>? logger,
        bool ownsHttpClient)
    {
        _settings = settings;
        _httpClient = httpClient;
        _logger = logger;
        _ownsHttpClient = ownsHttpClient;
    }

    public Task WriteTracesAsync(IReadOnlyList<TraceRow> rows, CancellationToken cancellationToken = default)
        => InsertAsync("traces", rows, MapTrace, cancellationToken);

    public Task WriteLogsAsync(IReadOnlyList<LogRow> rows, CancellationToken cancellationToken = default)
        => InsertAsync("logs", rows, MapLog, cancellationToken);

    public Task WriteCostsAsync(IReadOnlyList<CostRow> rows, CancellationToken cancellationToken = default)
        => InsertAsync("costs", rows, MapCost, cancellationToken);

    private async Task InsertAsync<T>(
        string table,
        IReadOnlyList<T> rows,
        Func<T, object> map,
        CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
        {
            return;
        }

        _logger?.LogDebug(
            "ClickHouse insert {Count} row(s) into {Table} via {Host}:{Port}",
            rows.Count,
            table,
            _settings.Host,
            _settings.Port);

        var body = BuildPayload(rows, map);
        var query =
            $"INSERT INTO {table} SETTINGS async_insert=0, wait_for_async_insert=1 FORMAT JSONEachRow";
        var requestUri = _settings.BuildRequestUri(query);

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        ApplyAuthHeaders(request);
        request.Headers.ConnectionClose = true;
        request.Content = new StringContent(body, Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            _logger?.LogInformation(
                "Inserted {Count} row(s) into {Database}.{Table}",
                rows.Count,
                _settings.Database,
                table);
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var message = $"ClickHouse insert into {table} failed with HTTP {(int)response.StatusCode}: {responseBody.Trim()}";
        _logger?.LogError(
            "ClickHouse insert failed ({StatusCode}) for {Database}.{Table}: {ResponseBody}",
            (int)response.StatusCode,
            _settings.Database,
            table,
            responseBody.Trim());

        throw new ClickHouseInsertException(message, (int)response.StatusCode, responseBody);
    }

    private static string BuildPayload<T>(IReadOnlyList<T> rows, Func<T, object> map)
    {
        var sb = new StringBuilder(rows.Count * 256);
        foreach (var row in rows)
        {
            sb.AppendLine(JsonSerializer.Serialize(map(row), JsonOptions));
        }

        return sb.ToString();
    }

    private static object MapTrace(TraceRow row)
        => new
        {
            event_time = FormatTime(row.EventTime),
            trace_id = row.TraceId,
            span_id = row.SpanId,
            parent_span_id = row.ParentSpanId,
            service_name = row.ServiceName,
            operation = row.Operation,
            provider = row.Provider,
            model_name = row.ModelName,
            status = row.Status,
            duration_ms = row.DurationMs,
            prompt_tokens = row.PromptTokens,
            completion_tokens = row.CompletionTokens,
            total_tokens = row.TotalTokens,
            estimated_cost_usd = row.EstimatedCostUsd,
            tenant_id = row.TenantId,
            attributes = row.Attributes.Count > 0 ? row.Attributes : null
        };

    private static object MapLog(LogRow row)
        => new
        {
            event_time = FormatTime(row.EventTime),
            trace_id = row.TraceId,
            severity = row.Severity,
            message = row.Message,
            attributes = row.Attributes.Count > 0 ? row.Attributes : null
        };

    private static object MapCost(CostRow row)
        => new
        {
            event_time = FormatTime(row.EventTime),
            tenant_id = row.TenantId,
            api_key_hash = row.ApiKeyHash,
            provider = row.Provider,
            model_name = row.ModelName,
            prompt_tokens = row.PromptTokens,
            completion_tokens = row.CompletionTokens,
            total_tokens = row.TotalTokens,
            cost_usd = row.CostUsd,
            currency = row.Currency
        };

    private static string FormatTime(DateTimeOffset value)
        => value.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");

    private void ApplyAuthHeaders(HttpRequestMessage request)
    {
        request.Headers.TryAddWithoutValidation("X-ClickHouse-User", _settings.Username);
        if (!string.IsNullOrEmpty(_settings.Password))
        {
            request.Headers.TryAddWithoutValidation("X-ClickHouse-Key", _settings.Password);
        }
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }
}
