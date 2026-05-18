# ClickHouse dashboard queries (v1.1+)

Use database `smartllm_telemetry` in DBeaver, Grafana ClickHouse plugin, or `clickhouse-client`.

## Requests per model (24h)

```sql
SELECT
    model_name,
    count() AS requests,
    sum(total_tokens) AS tokens,
    round(avg(duration_ms), 2) AS avg_latency_ms
FROM traces
WHERE event_time > now() - INTERVAL 24 HOUR
GROUP BY model_name
ORDER BY requests DESC;
```

## Cost by tenant (when `costs` populated)

```sql
SELECT
    tenant_id,
    sum(cost_usd) AS total_usd,
    sum(total_tokens) AS tokens
FROM costs
WHERE event_time > now() - INTERVAL 7 DAY
GROUP BY tenant_id
ORDER BY total_usd DESC;
```

## Tool usage (chat with tools)

```sql
SELECT
    event_time,
    model_name,
    attributes['smartllm.tool_call_count'] AS tool_calls
FROM traces
WHERE attributes['smartllm.tool_call_count'] != ''
ORDER BY event_time DESC
LIMIT 20;
```

## Embeddings volume

```sql
SELECT
    model_name,
    count() AS embed_requests,
    sum(total_tokens) AS tokens
FROM traces
WHERE operation = 'embeddings'
  AND event_time > now() - INTERVAL 24 HOUR
GROUP BY model_name;
```

## Errors

```sql
SELECT event_time, model_name, status, operation, trace_id
FROM traces
WHERE status = 'error'
ORDER BY event_time DESC
LIMIT 50;
```
