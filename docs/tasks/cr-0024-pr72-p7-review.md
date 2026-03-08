# Code Review: cr-0024 — PR #72 Epic P7 Intelligence & Analytics

## Metadata
| Field | Value |
|-------|-------|
| PR | [#72](https://github.com/Monkey-D-Luisi/propely/pull/72) |
| Branch | `feat/p7-intelligence-analytics` → `main` |
| Reviewers | gemini-code-assist[bot], copilot-pull-request-reviewer[bot] |
| Files changed | 98 (+3774 / -85) |
| Services | ai-api, contacts-api, appointments-api, web |

## Changed Files Summary
- **ai-api**: Conversation context (Redis-backed sessions, entity memory), suggestion engine (5 rules + orchestrator), new SuggestionsController
- **contacts-api**: count-by-status endpoint for leads
- **appointments-api**: count-by-status + count-upcoming endpoints
- **web**: Dashboard page (KPI grid, charts, quick actions), SuggestionFeed, design system polish (16 files), sessionId propagation in CommandBar

---

## Section 1: Agent Review Findings

### Architecture & Clean Architecture
- Domain layers have zero framework deps ✓
- Dependencies flow inward ✓
- Controllers are thin (delegate to MediatR) ✓
- CQRS separation maintained ✓
- Correct namespace convention ✓

### Domain Design
- `Suggestion` is a sealed record (correct value object) ✓
- `ConversationSession` enforces window trimming invariant ✓
- `EntityMemory` is immutable (uses `with` expression) ✓

### API & Security
- Authorization via `RequireViewer` policy on all new endpoints ✓
- No secrets in code ✓
- Multi-tenancy: `GetTenantId()` used correctly ✓
- **F6**: Indirect prompt injection risk in conversation history injection (SUGGESTION — see below)

### Testing
- 20 new ai-api unit tests ✓
- 6 new appointments-api unit tests ✓
- 3 new contacts-api unit tests ✓
- 4 new web component tests ✓
- AAA pattern followed ✓
- **F5**: Unused `using` in StaleLeadsRuleTests (SHOULD_FIX)

### Frontend
- Semantic `primary-*` classes used ✓
- `bg-surface` used for page background ✓
- Border radius conventions correct (Task 7.4 fixed these) ✓
- Focus rings added (Task 7.4) ✓
- **F4**: Unused `useTranslations` in `KpiCard` (SHOULD_FIX)

### Code Quality
- **F1**: `TryExtractGuid` only parses strings, misses boxed `Guid` values (MUST_FIX)
- **F2**: `CountUpcoming` returns anonymous object instead of typed DTO (MUST_FIX)
- **F3**: `SuggestionEngine` XML doc claims "deduplicated" but doesn't deduplicate (SHOULD_FIX)

---

## Section 2: Review Comment Threads

### Gemini Code Assist

| # | File | Claim | Classification | Resolution |
|---|------|-------|----------------|------------|
| G1 | `OpenAiIntentClassifier.cs:133` | Indirect prompt injection via `exchange.ResultMessage` | SUGGESTION | Add security comment. Result messages are system-generated. Full mitigation out of scope. |
| G2 | `ExecuteActionCommandHandler.cs:137-168` | `IDictionary` cast fails for anonymous objects | FALSE_POSITIVE | All entity-creating handlers return `Dictionary<string, object?>`. Only query handlers return anonymous objects, which don't contain entity IDs. Cast works for its purpose. |

### GitHub Copilot

| # | File | Claim | Classification | Resolution |
|---|------|-------|----------------|------------|
| C1 | `ExecuteActionCommandHandler.cs:144-147` | `UpdateEntityMemory` cast fails for anonymous objects | FALSE_POSITIVE | Same as G2 — all mutating handlers use dictionaries. |
| C2 | `ExecuteActionCommandHandler.cs:157-166` | `TryExtractGuid` only handles strings, misses `Guid` | MUST_FIX | Add `Guid`/`Guid?` type checks before string parse. |
| C3 | `SuggestionEngine.cs:10-13` | XML doc says "deduplicated" but no dedup logic | SHOULD_FIX | Update comment to "aggregated results ordered by priority". |
| C4 | `AppointmentsController.cs:93-101` | `CountUpcoming` returns anonymous object | MUST_FIX | Use `CountUpcomingResponse(result)` typed DTO. |
| C5 | `KpiGrid.tsx:17-18` | Unused `useTranslations` in `KpiCard` | SHOULD_FIX | Remove unused hook call. |
| C6 | `StaleLeadsRuleTests.cs:4-9` | Unused `using NSubstitute.ExceptionExtensions` | SHOULD_FIX | Remove unused import. |

---

## Resolution Plan

### MUST_FIX
- [x] F1: Fix `TryExtractGuid` to handle `Guid`/`Guid?` values
- [x] F2: Return `CountUpcomingApiResponse(result)` instead of anonymous object
- [x] F8: Fix `(dashboard)/page.tsx` vs `(marketing)/page.tsx` route conflict — moved dashboard to `/dashboard` URL

### SHOULD_FIX
- [x] F3: Fix `SuggestionEngine` XML doc
- [x] F4: Remove unused `useTranslations` in `KpiCard`
- [x] F5: Remove unused `using` in `StaleLeadsRuleTests`

### SUGGESTION
- [x] F6: Add security comment re: prompt injection risk in conversation history

### FALSE_POSITIVE
- [x] F7 (G2/C1): `IDictionary` cast — documented rationale above. All entity-mutating handlers use typed dictionaries.
