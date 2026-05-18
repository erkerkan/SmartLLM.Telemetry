# Semantic Conventions

SmartLLM.Telemetry follows OpenTelemetry semantic conventions where applicable and extends with `smartllm.*` attributes for generative AI.

## Activity naming

| Operation | Activity name |
|-----------|---------------|
| Chat completion | `smartllm.chat` |
| Embedding | `smartllm.embeddings` |
| Streaming chunk (optional child) | `smartllm.chat.chunk` |

## Required attributes (MVP)

| Attribute | Type | Description |
|-----------|------|-------------|
| `smartllm.provider` | string | `openai`, `azure_openai`, `ollama`, `lmstudio` |
| `smartllm.model_name` | string | Model identifier |
| `smartllm.operation` | string | `chat`, `embeddings` |
| `smartllm.request_id` | string | Correlation id |
| `smartllm.status` | string | `ok`, `error`, `cancelled` |

## Token & cost attributes

| Attribute | Type | Description |
|-----------|------|-------------|
| `smartllm.prompt_tokens` | int | Input tokens |
| `smartllm.completion_tokens` | int | Output tokens |
| `smartllm.total_tokens` | int | Sum |
| `smartllm.estimated_cost_usd` | double | Offline estimate |

## Optional attributes

| Attribute | Type | When |
|-----------|------|------|
| `smartllm.tenant_id` | string | Multi-tenant apps |
| `smartllm.api_key_id` | string | Hashed key id, never raw key |
| `smartllm.prompt_version` | string | A/B testing (Phase 4) |
| `gen_ai.system` | string | Align with OTel GenAI (future) |

## Events

| Event name | Fields |
|------------|--------|
| `smartllm.prompt` | `content` (only if CapturePrompts) |
| `smartllm.completion` | `content` (only if CapturePrompts) |

## Status mapping

- HTTP 2xx + successful body → `ActivityStatusCode.Ok`
- Provider error → `ActivityStatusCode.Error` + `smartllm.error.type`
- `OperationCanceledException` → `cancelled` tag + `Error` with message "cancelled"

## Provider behavior matrix (reference)

| Capability | OpenAI | Azure OpenAI | Ollama | LM Studio |
|------------|--------|--------------|--------|-----------|
| Real HTTP client | Yes | Yes | Yes | Yes (OpenAI-compatible) |
| Stub when unavailable | Key missing | Creds missing | Serve down | No |
| Streaming (`IChatClient`) | Yes | Yes | Yes | Yes |
| `ILlmClient` | Yes | Yes | Yes | Yes |
| Tool calling | Yes | Yes | Varies | Varies |
| Token usage in response | Yes | Yes | Often no | Often no |
| Offline token fallback | Yes | Yes | Yes | Yes |
| Cost in ClickHouse `costs` | When priced | When priced | Usually no | Usually no |
| ClickHouse export PII redact | With `AddSmartLLMSecurity()` | Same | Same | Same |
