# Task: 0053-typed-action-parameter-records

## Metadata
- ID: 0053
- Type: Standard
- Status: DOING
- Owner: Agent
- Created: 2026-03-08
- Epic: P8 — AI Provider Abstraction & MCP Server (Task 8.2)
- Related docs:
  - Walkthrough: `docs/walkthroughs/0053-typed-action-parameter-records.md`
  - Epic: `docs/backlog/epic-P8-ai-provider-abstraction.md`

## Goal
Replace untyped `Dictionary<string, object?>` parameter passing with 20 strongly-typed parameter records across all action commands and handlers, and delete the manual `ParameterExtractor` helper.

## Context
All 20 action handlers currently receive `Dictionary<string, object?>` and use `ParameterExtractor.GetString/GetInt/GetDecimal` to parse values — error-prone, untyped, and violates the typed philosophy of the rest of the codebase. Task 8.1 introduced `ToolSchemaRegistry` with provider-neutral JSON Schemas that define each action's parameters.

## Scope
### In scope
- Create `IActionParameters` marker interface
- Create 20 typed parameter records (one per action type) with strongly-typed properties
- Create `IParameterBinder` interface and `DefaultParameterBinder` implementation
- Refactor `ActionRouter` to bind parameters before dispatching
- Refactor all 20 action commands to accept typed parameter records
- Refactor all 20 action handlers to use typed properties
- Delete `ParameterExtractor.cs`

### Out of scope
- Changing action behavior (same inputs → same outputs)
- Adding new validation rules beyond what ParameterExtractor already does
- Changing parameter names from the OpenAI JSON Schema convention

## Requirements
- R1: Each of the 20 action types has a corresponding `*Parameters` record with typed properties
- R2: `IParameterBinder.Bind<T>()` converts `Dictionary<string,object?>` to typed records
- R3: All 20 action commands accept typed parameter records
- R4: All 20 action handlers use typed properties directly
- R5: `ParameterExtractor.cs` is deleted with no remaining references
- R6: All existing tests pass — behavioral equivalence

## Acceptance Criteria
- [ ] Each of the 20 action types has a corresponding `*Parameters` record with typed properties
- [ ] `IParameterBinder.Bind<T>(Dictionary<string,object?> raw)` returns a typed record
- [ ] All 20 action commands accept typed parameter records instead of `Dictionary<string,object?>`
- [ ] All 20 action handlers use typed properties (e.g., `parameters.City` instead of `ParameterExtractor.GetString(parameters, "city")`)
- [ ] `ParameterExtractor.cs` is deleted — no references remain
- [ ] All existing tests pass — behavioral equivalence guaranteed
- [ ] No `Dictionary<string,object?>` in any action command or handler (except in `ClassifiedIntent`, which remains for backward compat)

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.
- TDD: write tests first.

## Proposed Approach (high-level)
1. Create `IActionParameters` marker and 20 typed records in Application layer
2. Create `IParameterBinder` in Application, `DefaultParameterBinder` in Infrastructure
3. Binder uses `System.Text.Json` serialization: Dictionary → JSON → deserialize to typed record (leveraging `JsonNamingPolicy.SnakeCaseLower`)
4. Update `ActionRouter` to bind parameters before dispatching
5. Refactor commands and handlers one category at a time
6. Delete `ParameterExtractor.cs`

## Implementation Steps
1. Create `IActionParameters` marker interface
2. Create all 20 typed parameter records
3. Create `IParameterBinder` interface
4. Write `DefaultParameterBinderTests` (TDD Red)
5. Create `DefaultParameterBinder` (TDD Green)
6. Update `ActionRouter` to use binder
7. Refactor commands + handlers (property, content, contact, appointment, operation)
8. Delete `ParameterExtractor.cs`
9. Run full test suite

## Files to Create / Modify
- See epic task 8.2 file list in `docs/backlog/epic-P8-ai-provider-abstraction.md`

## Testing Plan
- Unit tests: `DefaultParameterBinderTests` — every type conversion (string, int, decimal, Guid, DateTime, List<string>), snake_case mapping, null handling
- Unit tests: Updated handler tests using typed parameter records
- Integration tests: Existing pipeline tests pass unchanged

## Security & Privacy
- `DefaultParameterBinder` rejects unknown properties silently (no injection)
- Type coercion via `System.Text.Json` is strict by default

## Observability
- Logs: Parameter binding logged at Debug level in ActionRouter
- No new metrics or traces needed

## Rollback Plan
Revert the commit — all changes are in ai-api only.

## Definition of Done Checklist
- [ ] Acceptance criteria met
- [ ] Build passes
- [ ] Tests added/updated and pass
- [ ] Formatting/analyzers pass
- [ ] No secrets committed
- [ ] Walkthrough updated
