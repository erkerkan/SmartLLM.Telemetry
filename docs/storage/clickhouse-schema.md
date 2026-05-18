# ClickHouse Schema

## Troubleshooting inserts

### DBeaver / JDBC: `Table default.traces does not exist`

Set connection database to `smartllm_telemetry`, or run `USE smartllm_telemetry;` before `INSERT INTO traces`.

### DBeaver / JDBC: `Code: 1001` / `transport error: 500`

Often hides:

```text
filesystem error: in rename: Permission denied
/var/lib/clickhouse/store/.../tmp_insert_.../
```

On **Docker Desktop for Windows**, bind-mounting a host folder (e.g. `C:\dockercompose\clickhouse\ch_data`) breaks MergeTree inserts. `chown` inside the container usually does not fix it.

**Fix:** use a Docker named volume instead of a Windows path bind mount. Repo compose file:

[`docker/clickhouse/docker-compose.yml`](../../docker/clickhouse/docker-compose.yml)

```bash
cd docker/clickhouse
docker compose up -d
# apply Schema/001_init.sql
```

**Verify SQL is fine** (Memory engine, no disk):

```sql
CREATE TABLE smartllm_telemetry.traces_mem ENGINE = Memory AS
SELECT * FROM smartllm_telemetry.traces WHERE 1 = 0;

INSERT INTO smartllm_telemetry.traces_mem
(event_time, trace_id, span_id, parent_span_id, service_name, operation, provider, model_name, status, duration_ms, prompt_tokens, completion_tokens, total_tokens, estimated_cost_usd, tenant_id)
VALUES (now64(3), 'mem-test', 's1', '', 'svc', 'op', 'p', 'm', 'ok', 1, 1, 1, 2, 0.0, '');

SELECT trace_id FROM smartllm_telemetry.traces_mem;
```

If this works but `traces` (MergeTree) fails, the problem is storage, not SQL.

### DateTime in JSONEachRow

Use `yyyy-MM-dd HH:mm:ss.fff` (UTC) for `DateTime64(3, 'UTC')`; do not use ISO `Z` suffix.

The sink logs the full ClickHouse HTTP response body on insert failure.

## Database

`smartllm_telemetry`

## traces

```sql
CREATE TABLE IF NOT EXISTS smartllm_telemetry.traces
(
    event_time DateTime64(3, 'UTC'),
    trace_id String,
    span_id String,
    parent_span_id String,
    service_name LowCardinality(String),
    operation LowCardinality(String),
    provider LowCardinality(String),
    model_name LowCardinality(String),
    status LowCardinality(String),
    duration_ms UInt32,
    prompt_tokens UInt32,
    completion_tokens UInt32,
    total_tokens UInt32,
    estimated_cost_usd Float64,
    tenant_id String DEFAULT '',
    attributes Map(String, String) DEFAULT map()
)
ENGINE = MergeTree()
PARTITION BY toYYYYMM(event_time)
ORDER BY (service_name, event_time, trace_id)
TTL event_time + INTERVAL 30 DAY;
```

## logs

```sql
CREATE TABLE IF NOT EXISTS smartllm_telemetry.logs
(
    event_time DateTime64(3, 'UTC'),
    trace_id String,
    severity LowCardinality(String),
    message String,
    attributes Map(String, String) DEFAULT map()
)
ENGINE = MergeTree()
PARTITION BY toYYYYMM(event_time)
ORDER BY (event_time, trace_id)
TTL event_time + INTERVAL 14 DAY;
```

## costs

```sql
CREATE TABLE IF NOT EXISTS smartllm_telemetry.costs
(
    event_time DateTime64(3, 'UTC'),
    tenant_id String,
    api_key_hash String,
    provider LowCardinality(String),
    model_name LowCardinality(String),
    prompt_tokens UInt32,
    completion_tokens UInt32,
    total_tokens UInt32,
    cost_usd Float64,
    currency FixedString(3) DEFAULT 'USD'
)
ENGINE = MergeTree()
PARTITION BY toYYYYMM(event_time)
ORDER BY (tenant_id, event_time)
TTL event_time + INTERVAL 365 DAY;
```

## Migrations

SQL scripts live in `src/SmartLLM.Telemetry.Sinks.ClickHouse/Schema/`. Apply via `clickhouse-client` or deployment pipeline.

## Vector search (Phase 2)

Separate table `semantic_cache_embeddings` with `Array(Float32)` and vector index — documented in Caching.Semantic package.
