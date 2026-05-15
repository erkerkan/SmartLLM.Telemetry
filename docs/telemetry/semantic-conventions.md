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
| `smartllm.provider` | string | `openai`, `azure_openai`, `ollama` |
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

| Capability | OpenAI | Azure OpenAI | Ollama |
|------------|--------|--------------|--------|
| Streaming | Yes | Yes | Yes |
| Tool calling | Yes | Yes | Varies |
| Token usage in response | Yes | Yes | Often no |
| Offline token fallback | Yes | Yes | Yes |
