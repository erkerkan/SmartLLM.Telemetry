CREATE DATABASE IF NOT EXISTS smartllm_telemetry;

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
