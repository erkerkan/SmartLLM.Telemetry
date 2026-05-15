# Data Retention and Compliance

## Retention defaults (ClickHouse)

| Table | Recommended TTL | Notes |
|-------|-----------------|-------|
| `traces` | 30 days | Raw spans; high volume |
| `logs` | 14 days | Structured events |
| `costs` | 365 days | Finance reporting |

Adjust per tenant via ClickHouse TTL clauses on `event_time`.

## Tenant isolation

- Include `tenant_id` in all fact tables.
- Row policies in ClickHouse (deployment concern) restrict query access.
- API keys stored as `api_key_hash` (SHA-256 prefix), never plaintext.

## Right to erasure

Provide deletion by `tenant_id` + `user_id` hash within SLA (implementation in sink/admin tooling, Phase 3).

## Audit trail

Quota blocks and policy denials should emit `smartllm.policy.blocked` activities with reason code (Epic 8).

## Deployment checklist

- [ ] `CapturePrompts=false` in production unless legally approved
- [ ] TLS to ClickHouse and providers
- [ ] Secrets via environment / Key Vault, not appsettings in repo
- [ ] Retention TTL applied on all telemetry tables
