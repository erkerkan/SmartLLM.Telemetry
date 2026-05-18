# ClickHouse schema migrations

## Baseline

Apply once on a new database:

`src/SmartLLM.Telemetry.Sinks.ClickHouse/Schema/001_init.sql`

Creates `smartllm_telemetry` with `traces`, `logs`, and `costs`.

## Adding a migration

1. Add `Schema/00N_description.sql` (never edit `001` after release).
2. Document the change in `CHANGELOG.md`.
3. Apply manually in maintenance window (no auto-runner in SDK v1.0).

Example `002_add_column.sql` template:

```sql
-- Example only — not applied by default
ALTER TABLE smartllm_telemetry.traces
    ADD COLUMN IF NOT EXISTS environment LowCardinality(String) DEFAULT '';
```

## Rollback

ClickHouse `ALTER` rollbacks are manual. Prefer additive columns with defaults.

## Version compatibility

| SDK version | Schema |
|-------------|--------|
| 1.0.x | `001_init.sql` |
