# Semantic Conventions

SmartLLM.Telemetry follows OpenTelemetry semantic conventions where applicable and extends with `smartllm.*` attributes for generative AI.

## Activity naming

| Operation | Activity name |
|-----------|---------------|
| Chat completion | `smartllm.chat` |
| Tool call (child) | `smartllm.tool` |
| Embedding | `smartllm.embeddings` |
| Streaming chunk (optional child) | `smartllm.chat.chunk` |

## Required attributes (MVP)

| Attribute | Type | Description |
|-----------|------|-------------|
| `smartllm.provider` | string | `openai`, `azure_openai`, `ollama`, `lmstudio` |
| `smartllm.model_name` | string | Model identifier |
| `smartllm.operation` | string | `chat`, `chat_with_tools`, `embeddings`, `tool_call` |
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
| `smartllm.tool_call_count` | int | Assistant tool calls in response |
| `smartllm.tool_result_count` | int | Tool-role messages in request |
| `smartllm.tool.name` | string | On `smartllm.tool` child span |
| `smartllm.embedding.input_count` | int | Embedding batch size |
| `gen_ai.system` | string | Align with OTel GenAI (future) |

## Events

| Event name | Fields |
|------------|--------|
| `smartllm.prompt` | `content` (only if CapturePrompts) |
| `smartllm.completion` | `content` (only if CapturePrompts) |
| `smartllm.tool_call` | `tool.name`, `tool.call_id`, `tool.arguments` (if CaptureToolArguments) |
| `smartllm.tool_result` | `tool.call_id`, `tool.result` (if CaptureToolResults) |

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
