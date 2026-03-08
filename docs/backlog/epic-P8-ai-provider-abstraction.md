# Epic P8 -- AI Provider Abstraction & MCP Server

## Overview

Decouple the AI Action Engine from OpenAI-specific types and expose it as an MCP (Model Context Protocol) server, enabling multi-LLM support and standard AI tool interoperability. This epic addresses three systemic issues identified during architectural review:

1. **Vendor lock-in**: `ToolDefinitions.cs` uses `OpenAI.Chat.ChatTool` directly — switching to Claude, Gemini, or any other provider requires rewriting all 20 tool definitions and the classifier.
2. **Untyped parameter passing**: All 19 action handlers receive `Dictionary<string,object?>` and use a manual `ParameterExtractor` to parse values — error-prone, untyped, and violates the typed philosophy of the rest of the codebase.
3. **Closed AI interface**: The action engine is only accessible via `POST /v1/actions/execute` with OpenAI as the sole classification backend. No standard protocol allows external AI agents to discover and invoke Propely's capabilities.

**This epic does NOT replace the NuGet SDK clients.** Refit SDK clients (with Polly resilience, tenant propagation, DTOs) remain the execution layer for inter-service communication. MCP is added as an AI interface layer on top of the existing pipeline.

## Architectural Context

### Current Pipeline (before P8)
```
POST /v1/actions/execute
  → OpenAiIntentClassifier (OpenAI.Chat.ChatTool, OpenAI.Chat.ChatClient)
    → ActionRouter (switch → MediatR)
      → Handler (Dictionary<string,object?> → ParameterExtractor → SDK Client)
        → HTTP to domain service
```

### Target Pipeline (after P8)
```
┌─ POST /v1/actions/execute (existing, unchanged) ──────────────┐
│  → IIntentClassifier (provider-agnostic interface)             │
│    → OpenAiClassifierAdapter (uses OpenAiToolAdapter)          │
│    → (future) ClaudeClassifierAdapter                          │
│    → (future) GeminiClassifierAdapter                          │
│                                                                │
├─ /mcp (new MCP Streamable HTTP endpoint) ──────────────────────┤
│  → MCP tools/list → returns 20 tool schemas                   │
│  → MCP tools/call → routes to ActionRouter                    │
│                                                                │
├─ Shared: ToolSchemaRegistry (provider-neutral JSON Schemas)    │
├─ Shared: ActionRouter + MediatR Handlers                       │
├─ Shared: Typed parameter records (replaces Dictionary)         │
└─ Shared: SDK Clients (Refit + Polly, unchanged)                │
```

## Service Ownership

| Capability | Service | Layer |
|---|---|---|
| Provider-neutral tool schemas | `services/ai-api` | Application |
| Provider adapters (OpenAI, future Claude/Gemini) | `services/ai-api` | Infrastructure |
| Typed action parameter records | `services/ai-api` | Application |
| MCP server endpoint | `services/ai-api` | Api + Infrastructure |
| Shared TenantDelegatingHandler | `services/shared/` | Shared package (new) |

## Tasks

| # | Title | Status | Dependencies | Estimated Effort |
|---|---|---|---|---|
| 8.1 | Provider-Neutral Tool Schema Registry | DONE | 3.1 (DONE) | Medium |
| 8.2 | Typed Action Parameter Records | DONE | 8.1 | Medium |
| 8.3 | OpenAI Classifier Adapter Refactor | PENDING | 8.1, 8.2 | Small |
| 8.4 | MCP Server Endpoint | PENDING | 8.1 | Medium-Large |
| 8.5 | Shared TenantDelegatingHandler Package | PENDING | — | Small |

---

## Task 8.1 — Provider-Neutral Tool Schema Registry

- **Status:** DONE
- **Dependencies:** 3.1 (DONE)

### Scope

#### In scope
- Extract all 20 tool definitions from `ToolDefinitions.cs` (OpenAI-coupled `ChatTool.CreateFunctionTool`) into a provider-neutral format
- Create `ToolSchema` record type: `Name`, `Description`, `ParametersJsonSchema` (raw JSON Schema string, including any required parameters), `ActionType`
- Create `ToolSchemaRegistry` that exposes `IReadOnlyList<ToolSchema> All` and `ToolSchema? GetByName(string name)`
- Maintain the `ActionType` resolution: `ToolSchemaRegistry.ResolveActionType(string toolName)` (same as current `ToolDefinitions.ResolveActionType`)
- Create `IToolAdapter` interface: `ConvertAll(IReadOnlyList<ToolSchema> schemas)` returns provider-specific tool objects
- Create `OpenAiToolAdapter : IToolAdapter` that converts `ToolSchema` → `ChatTool.CreateFunctionTool`
- Ensure all 20 JSON Schemas are identical to current definitions (bit-for-bit parameter compatibility)
- Remove old `ToolDefinitions.cs` after migration

#### Out of scope
- Claude or Gemini adapters (future tasks, not in this epic)
- Changing the JSON Schema content (parameter names, types, descriptions stay the same)
- Moving tool schemas to external files (keep as embedded C# strings for now — simpler, no file I/O)

### Acceptance Criteria
- [x] `ToolSchemaRegistry` exposes all 20 tool schemas with correct names, descriptions, and JSON Schemas
- [x] `ToolSchemaRegistry.ResolveActionType` returns correct `ActionType` for all 20 function names
- [x] `OpenAiToolAdapter` converts all 20 schemas into `ChatTool` objects identical to the current `ToolDefinitions.All`
- [x] `ToolDefinitions.cs` is deleted — no references to `ChatTool.CreateFunctionTool` outside the adapter
- [x] All existing unit and integration tests pass without modification (behavioral equivalence)
- [x] No OpenAI-specific types in Application layer (only in Infrastructure adapter)

### Implementation Steps
1. Create `ToolSchema` record in Application layer: `Propely.AiApi.Application.Actions.Tools.ToolSchema`
2. Create `IToolSchemaRegistry` interface in Application layer
3. Create `ToolSchemaRegistry` implementation in Application layer with all 20 schemas as provider-neutral records
4. Create `IToolAdapter` interface in Application layer: `IReadOnlyList<object> ConvertAll(IReadOnlyList<ToolSchema> schemas)`
5. Create `OpenAiToolAdapter` in Infrastructure layer: implements `IToolAdapter`, converts `ToolSchema` → `ChatTool`
6. Update `OpenAiIntentClassifier` to inject `IToolSchemaRegistry` + `OpenAiToolAdapter` instead of referencing `ToolDefinitions.All` directly
7. Write unit tests for `ToolSchemaRegistry` (all 20 schemas present, correct names, ActionType resolution)
8. Write unit tests for `OpenAiToolAdapter` (verify output matches current `ToolDefinitions.All`)
9. Delete `ToolDefinitions.cs`
10. Run full test suite — verify zero regressions

### Files to Create
- `services/ai-api/src/Propely.AiApi.Application/Actions/Tools/ToolSchema.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Tools/IToolSchemaRegistry.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Tools/ToolSchemaRegistry.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Tools/IToolAdapter.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/AI/Adapters/OpenAiToolAdapter.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Actions/Tools/ToolSchemaRegistryTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/AI/Adapters/OpenAiToolAdapterTests.cs`

### Files to Modify
- `services/ai-api/src/Propely.AiApi.Infrastructure/AI/OpenAiIntentClassifier.cs` — inject `IToolSchemaRegistry` + `OpenAiToolAdapter`
- `services/ai-api/src/Propely.AiApi.Infrastructure/DependencyInjection.cs` — register new services

### Files to Delete
- `services/ai-api/src/Propely.AiApi.Infrastructure/AI/ToolDefinitions.cs`

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | All 20 schemas in registry | Assert name, description, JSON schema presence |
| Unit | ActionType resolution for all 20 function names | Parameterized test |
| Unit | OpenAiToolAdapter output matches original ChatTool definitions | Serialize both and compare |
| Unit | OpenAiIntentClassifier still works with injected adapter | Mock adapter, verify tool list passed to ChatClient |
| Integration | End-to-end action pipeline with schema registry | Same integration tests as before — behavioral equivalence |

### Security & Privacy
- No new attack surface — same tool definitions, same behavior
- JSON Schemas validated at startup (fail-fast if malformed)

### TDD Reminder
Red-Green-Refactor: Write failing tests for `ToolSchemaRegistry` and `OpenAiToolAdapter` first, then implement.

---

## Task 8.2 — Typed Action Parameter Records

- **Status:** DONE
- **Dependencies:** 8.1

### Scope

#### In scope
- Create a typed parameter record for each of the 20 action types (e.g., `CreatePropertyParameters`, `BookViewingParameters`, `QueryLeadsParameters`)
- Each record has strongly-typed properties matching the JSON Schema from 8.1 (e.g., `string? City`, `int? Bedrooms`, `decimal? Price`)
- Create `IParameterBinder` interface that converts `Dictionary<string,object?>` → typed record
- Create `DefaultParameterBinder` implementation using `System.Text.Json` deserialization (snake_case → PascalCase)
- Refactor all 19 action command records to accept the typed parameter record instead of `Dictionary<string,object?>`
- Refactor all 19 action handler classes to use typed properties instead of `ParameterExtractor.GetString/GetInt/GetDecimal`
- Delete `ParameterExtractor.cs` after all handlers are migrated
- Update `ActionRouter` to bind parameters before dispatching

#### Out of scope
- Changing action behavior (same inputs → same outputs)
- Adding new validation rules beyond what `ParameterExtractor` already does
- Changing parameter names from the OpenAI JSON Schema convention

### Acceptance Criteria
- [ ] Each of the 20 action types has a corresponding `*Parameters` record with typed properties
- [ ] `IParameterBinder.Bind<T>(Dictionary<string,object?> raw)` returns a typed record
- [ ] All 19 action commands accept typed parameter records instead of `Dictionary<string,object?>`
- [ ] All 19 action handlers use typed properties (e.g., `parameters.City` instead of `ParameterExtractor.GetString(parameters, "city")`)
- [ ] `ParameterExtractor.cs` is deleted — no references remain
- [ ] All existing tests pass — behavioral equivalence guaranteed
- [ ] No `Dictionary<string,object?>` in any action command or handler (except in `ClassifiedIntent`, which remains for backward compat)

### Implementation Steps
1. Define base `IActionParameters` marker interface in Application layer
2. Create all 20 typed parameter records (one per action type) in `Propely.AiApi.Application.Actions.Parameters/`
3. Create `IParameterBinder` interface in Application layer
4. Create `DefaultParameterBinder` in Infrastructure layer (handles snake_case JSON → PascalCase records, null-safety, type coercion)
5. Write unit tests for `DefaultParameterBinder` — test every type conversion (string, int, decimal, Guid, DateTime, enum, List<string>)
6. Update `ActionRouter` to call `IParameterBinder.Bind<T>` before dispatching to MediatR
7. Refactor action commands one category at a time:
   a. Property actions (4 commands) — verify tests pass
   b. Content actions (3 commands) — verify tests pass
   c. Contact/lead actions (5 commands) — verify tests pass
   d. Appointment actions (4 commands) — verify tests pass
   e. Operation actions (4 commands) — verify tests pass
8. Delete `ParameterExtractor.cs`
9. Run full test suite

### Files to Create
- `services/ai-api/src/Propely.AiApi.Application/Actions/Parameters/IActionParameters.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Parameters/CreatePropertyParameters.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Parameters/UpdatePropertyParameters.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Parameters/QueryPropertiesParameters.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Parameters/ChangePropertyStatusParameters.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Parameters/GenerateCopyParameters.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Parameters/ExtractFromTextParameters.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Parameters/ExtractFromPhotosParameters.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Parameters/CreateLeadParameters.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Parameters/CreateContactParameters.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Parameters/QualifyLeadParameters.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Parameters/ConvertLeadParameters.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Parameters/QueryLeadsParameters.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Parameters/BookViewingParameters.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Parameters/QueryAppointmentsParameters.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Parameters/CancelAppointmentParameters.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Parameters/RescheduleAppointmentParameters.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Parameters/ReservePropertyParameters.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Parameters/CloseOperationParameters.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Parameters/ArchivePropertyParameters.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Parameters/ReactivatePropertyParameters.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Interfaces/IParameterBinder.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/AI/DefaultParameterBinder.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/AI/DefaultParameterBinderTests.cs`

### Files to Modify
- `services/ai-api/src/Propely.AiApi.Infrastructure/AI/ActionRouter.cs` — add parameter binding before dispatch
- All 19 action command files — change constructor from `Dictionary` to typed record
- All 19 action handler files — use typed properties instead of `ParameterExtractor`
- `services/ai-api/src/Propely.AiApi.Infrastructure/DependencyInjection.cs` — register `IParameterBinder`

### Files to Delete
- `services/ai-api/src/Propely.AiApi.Application/Actions/Helpers/ParameterExtractor.cs`

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | DefaultParameterBinder type conversions | Every type: string, int, decimal, Guid, DateTime, enum, List<string>, nullable handling |
| Unit | DefaultParameterBinder snake_case → PascalCase mapping | `property_type` → `PropertyType`, `min_bedrooms` → `MinBedrooms` |
| Unit | All 19 handlers with typed parameters | Same test logic, different parameter construction |
| Integration | End-to-end pipeline | Same integration tests — behavioral equivalence |

### Security & Privacy
- `DefaultParameterBinder` must reject unknown properties silently (no injection via extra fields)
- Type coercion must be strict — no silent data loss (e.g., truncating a long to int must throw, not silently overflow)

### TDD Reminder
Start with `DefaultParameterBinderTests` covering all type conversions, then implement the binder.

---

## Task 8.3 — OpenAI Classifier Adapter Refactor

- **Status:** PENDING
- **Dependencies:** 8.1, 8.2

### Scope

#### In scope
- Extract `IIntentClassifier` into a provider-agnostic pattern where the classifier is decoupled from OpenAI SDK types
- Refactor `OpenAiIntentClassifier` to use `IToolSchemaRegistry` (from 8.1) + `OpenAiToolAdapter` (from 8.1) instead of importing `ToolDefinitions` directly
- The system prompt in `OpenAiIntentClassifier` stays as-is (it's content, not coupling)
- Ensure `IIntentClassifier` interface remains unchanged (it's already provider-agnostic)
- Register classifier via keyed DI or options pattern (e.g., `"OpenAI"` key) to prepare for multi-provider support

#### Out of scope
- Implementing a Claude or Gemini classifier (not in this epic)
- Changing the system prompt
- Multi-provider runtime switching (future — just prepare the DI structure)

### Acceptance Criteria
- [ ] `OpenAiIntentClassifier` does not reference `ToolDefinitions` class at all
- [ ] `OpenAiIntentClassifier` gets tools via `IToolSchemaRegistry` + `OpenAiToolAdapter`
- [ ] `IIntentClassifier` interface unchanged (backward compatible)
- [ ] No OpenAI types in Application layer (all in Infrastructure)
- [ ] DI registration uses named/keyed service or explicit configuration for provider selection
- [ ] All existing tests pass

### Implementation Steps
1. Update `OpenAiIntentClassifier` constructor to inject `IToolSchemaRegistry` and `OpenAiToolAdapter`
2. In `ClassifyAsync`, use `adapter.ConvertAll(registry.All)` instead of `ToolDefinitions.All`
3. Use `registry.ResolveActionType(functionName)` instead of `ToolDefinitions.ResolveActionType`
4. Register classifier with a provider key in DI: `services.AddKeyedScoped<IIntentClassifier, OpenAiIntentClassifier>("OpenAI")`
5. Add configuration option `AiProvider` (default: `"OpenAI"`) in appsettings
6. Update `DependencyInjection.cs` to resolve the configured provider
7. Verify all tests pass

### Files to Modify
- `services/ai-api/src/Propely.AiApi.Infrastructure/AI/OpenAiIntentClassifier.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/DependencyInjection.cs`
- `services/ai-api/src/Propely.AiApi.Api/appsettings.json` (add `AiProvider` config)

### Files to Create
- `services/ai-api/tests/Propely.AiApi.UnitTests/AI/OpenAiIntentClassifierAdapterTests.cs` (verify adapter injection works)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | Classifier uses injected adapter, not static ToolDefinitions | Mock adapter, verify tools passed to ChatClient |
| Unit | Provider key DI resolution | Resolve by key, verify correct type returned |
| Integration | Full classify pipeline | Same existing tests — behavioral equivalence |

### Security & Privacy
- No new attack surface
- Provider configuration validated at startup (unknown provider = fail fast)

### TDD Reminder
Write test verifying `OpenAiIntentClassifier` no longer imports `ToolDefinitions`, then refactor.

---

## Task 8.4 — MCP Server Endpoint

- **Status:** PENDING
- **Dependencies:** 8.1

### Scope

#### In scope
- Add `ModelContextProtocol` NuGet package to ai-api
- Mount an MCP Streamable HTTP endpoint at `/mcp` in `Program.cs`
- Register all 20 tools from `IToolSchemaRegistry` as MCP tools
- Implement MCP `tools/call` handler that:
  1. Receives tool name + JSON arguments
  2. Resolves `ActionType` via `IToolSchemaRegistry.ResolveActionType`
  3. Constructs `ClassifiedIntent` with the parameters
  4. Delegates to `IActionRouter.RouteAsync` (reusing the entire existing pipeline)
  5. Returns structured result
- Implement MCP `tools/list` handler that returns all 20 tool schemas
- Authentication: require Bearer token on the MCP endpoint (same JWT auth as REST endpoints)
- Tenant context: extract `X-Tenant-Id` from MCP request headers (or from JWT claims)

#### Out of scope
- MCP resources (exposing data as resources — future enhancement)
- MCP prompts (exposing prompt templates — future enhancement)
- MCP stdio transport (we only need HTTP for server deployment)
- Removing the existing REST endpoint (it stays alongside MCP)
- Client-side MCP integration in web frontend (separate task, not in this epic)

### Acceptance Criteria
- [ ] `/mcp` endpoint responds to MCP protocol handshake (initialize / initialized)
- [ ] `tools/list` returns all 20 tool schemas with correct names, descriptions, and inputSchema
- [ ] `tools/call` with valid tool name + arguments executes the action and returns result
- [ ] `tools/call` with unknown tool name returns MCP error response
- [ ] Authentication enforced: unauthenticated requests return 401
- [ ] Tenant isolation enforced: X-Tenant-Id propagated to SDK client calls
- [ ] MCP endpoint coexists with existing REST endpoints without interference
- [ ] Integration test: MCP client → tools/list → tools/call → verify result matches REST endpoint result

### Implementation Steps
1. Add `ModelContextProtocol` NuGet package to `Propely.AiApi.Api.csproj`
2. Create `McpToolHandler` in Infrastructure layer:
   - Inject `IToolSchemaRegistry`, `IActionRouter`
   - `HandleToolCallAsync(string toolName, JsonElement arguments, Guid tenantId, Guid agentId)` → `ActionResult`
3. Create MCP server configuration in Infrastructure:
   - Register tools from `IToolSchemaRegistry`
   - Map tool calls to `McpToolHandler`
4. Mount MCP endpoint in `Program.cs`:
   - `app.MapMcp("/mcp")` (or equivalent from the .NET MCP SDK)
   - Apply authentication middleware
5. Implement tenant context extraction from MCP request
6. Write integration test with in-process MCP client
7. Test manually with a real MCP client (e.g., Claude Desktop config pointing to local endpoint)

### Files to Create
- `services/ai-api/src/Propely.AiApi.Infrastructure/MCP/McpToolHandler.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/MCP/PropelyMcpServerConfiguration.cs`
- `services/ai-api/tests/Propely.AiApi.IntegrationTests/MCP/McpEndpointTests.cs`

### Files to Modify
- `services/ai-api/src/Propely.AiApi.Api/Propely.AiApi.Api.csproj` — add `ModelContextProtocol` package
- `services/ai-api/src/Propely.AiApi.Api/Program.cs` — mount MCP endpoint
- `services/ai-api/src/Propely.AiApi.Infrastructure/DependencyInjection.cs` — register MCP services

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | McpToolHandler routes to ActionRouter correctly | Mock router, verify parameters |
| Unit | Tool schema conversion to MCP format | Verify all 20 tools registered |
| Integration | MCP tools/list returns all tools | In-process MCP client |
| Integration | MCP tools/call executes action end-to-end | Call MCP → verify SDK client called |
| Integration | Unauthenticated MCP request returns 401 | HTTP test without token |
| Integration | Behavioral equivalence: MCP call = REST call | Same input via both paths, compare output |

### Security & Privacy
- MCP endpoint MUST require authentication (same JWT as REST endpoints)
- Tenant isolation MUST be enforced (requests without X-Tenant-Id are rejected)
- Rate limiting should be applied (same as REST endpoints)
- Tool arguments are validated against JSON Schema before execution

### TDD Reminder
Start with integration test: MCP client → tools/list → verify 20 tools returned. Then tools/call → verify action executed.

---

## Task 8.5 — Shared TenantDelegatingHandler Package

- **Status:** PENDING
- **Dependencies:** None (independent, can be done in parallel)

### Scope

#### In scope
- Create a shared NuGet package `Propely.Shared.Http` (or similar) containing `TenantDelegatingHandler`
- Move the shared implementation there — single source of truth
- Update all 5 SDK client projects to reference the shared package instead of their own copy
- Delete the 5 duplicate `TenantDelegatingHandler` files

#### Out of scope
- Changing `TenantDelegatingHandler` behavior
- Moving other shared code (resilience config, PagedResult) — future improvement
- Publishing as a real NuGet package (use ProjectReference in monorepo)

### Acceptance Criteria
- [ ] Single `TenantDelegatingHandler` in shared package
- [ ] All 5 SDK clients reference the shared package
- [ ] No duplicate `TenantDelegatingHandler` files remain in any Client project
- [ ] All existing tests pass — behavioral equivalence
- [ ] Docker build succeeds with the new project reference

### Implementation Steps
1. Create `services/shared/Propely.Shared.Http/` project
2. Move `TenantDelegatingHandler` to shared project
3. Add `ProjectReference` to shared project in all 5 SDK client `.csproj` files
4. Delete 5 duplicate handler files
5. Update Docker build context / volume mounts if needed
6. Run all solutions: `dotnet build` + `dotnet test`

### Files to Create
- `services/shared/Propely.Shared.Http/Propely.Shared.Http.csproj`
- `services/shared/Propely.Shared.Http/TenantDelegatingHandler.cs`

### Files to Delete
- `services/ai-api/src/Propely.AiApi.Client/TenantDelegatingHandler.cs`
- `services/orgs-api/src/Propely.OrgsApi.Client/TenantDelegatingHandler.cs`
- `services/properties-api/src/Propely.PropertiesApi.Client/TenantDelegatingHandler.cs`
- `services/contacts-api/src/Propely.ContactsApi.Client/TenantDelegatingHandler.cs`
- `services/appointments-api/src/Propely.AppointmentsApi.Client/TenantDelegatingHandler.cs`

### Files to Modify
- `services/ai-api/src/Propely.AiApi.Client/Propely.AiApi.Client.csproj` — add shared reference
- `services/orgs-api/src/Propely.OrgsApi.Client/Propely.OrgsApi.Client.csproj` — add shared reference
- `services/properties-api/src/Propely.PropertiesApi.Client/Propely.PropertiesApi.Client.csproj` — add shared reference
- `services/contacts-api/src/Propely.ContactsApi.Client/Propely.ContactsApi.Client.csproj` — add shared reference
- `services/appointments-api/src/Propely.AppointmentsApi.Client/Propely.AppointmentsApi.Client.csproj` — add shared reference
- `docker-compose.yml` — add volume mount for shared project if needed
- All 5 `ServiceCollectionExtensions.cs` — update `using` namespace

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | TenantDelegatingHandler propagates X-Tenant-Id | Same existing tests |
| Integration | SDK clients work with shared handler | Existing integration tests pass |
| Build | Docker build succeeds | `docker-compose build` |

### Security & Privacy
- No behavior change — same handler, just deduplicated

### TDD Reminder
Run all existing tests first (green baseline), then refactor, then run again (still green).

---

## Dependency Graph

```
                    8.5 (TenantDelegatingHandler)
                    (independent, parallel)

8.1 (Tool Schema Registry)
 ├── 8.2 (Typed Parameters) ── 8.3 (Classifier Adapter Refactor)
 └── 8.4 (MCP Server Endpoint)
```

Recommended execution order:
1. **8.5** (independent, quick win, can run in parallel with everything)
2. **8.1** (foundation — all other AI tasks depend on it)
3. **8.2** (typed params — improves handler quality, blocks 8.3)
4. **8.3** (classifier refactor — clean finish for provider decoupling)
5. **8.4** (MCP server — the crown jewel, requires 8.1)

Tasks 8.5 + 8.1 can be done in parallel. Tasks 8.2 and 8.4 can be done in parallel (both depend only on 8.1).
