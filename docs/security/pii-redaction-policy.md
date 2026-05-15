# PII Redaction Policy

## Data classification

| Level | Examples | Export behavior |
|-------|----------|-----------------|
| **L0 — Secret** | API keys, passwords, JWT | Never log or trace |
| **L1 — PII** | Email, phone, credit card, SSN | Mask before export |
| **L2 — Sensitive business** | Customer names, account ids | Hash or truncate (configurable) |
| **L3 — Operational** | Model name, token counts | Full export allowed |

## Default rules (Security package)

Built-in regex detectors:

- Email addresses
- Credit card numbers (Luhn-valid patterns)
- US SSN patterns
- IBAN (basic)

Replacement token: `[REDACTED:{type}]` e.g. `[REDACTED:email]`.

## Configuration

```csharp
services.AddSmartLLMSecurity(options =>
{
    options.Enabled = true;
    options.RedactInPrompts = true;
    options.RedactInCompletions = true;
    options.CustomPatterns.Add(new Regex(@"\\bEMP-\\d+\\b"), "employee_id");
});
```

## Prompt capture interaction

When `CapturePrompts=true`:

1. Redaction runs **before** span events are recorded.
2. Raw prompts must never reach ClickHouse without redaction when Security is enabled.

## AI-based redaction (Phase 2)

Optional classifier for unstructured PII; regex remains default for determinism and latency.

## Compliance notes

- GDPR: minimize L1/L2 in logs; define retention in [data-retention-and-compliance.md](data-retention-and-compliance.md).
- PCI: never store full PAN; L1 credit card rule is mandatory when handling payment contexts.
