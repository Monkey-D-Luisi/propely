# Walkthrough: 0049-work-items-frontend

## Task Reference
- Task: `docs/tasks/0049-work-items-frontend.md`
- Walkthrough: `docs/walkthroughs/0049-work-items-frontend.md`
- Branch/PR: `feat/0049-work-items-frontend`
- Date: `2026-02-13`

## Summary
Implemented full CRUD frontend for work items (AI API's primary domain entity) with list, create, detail, and edit views. Added AI Smart Fill feature: a `POST /v1/work-items/parse` backend endpoint that uses `IOpenAiService` to extract structured fields from natural language, plus a SmartFill component on the create form. Added search support to the ListWorkItems query.

## Context
- Background: The AI API has a fully functional WorkItems backend (`WorkItemsController` with CRUD endpoints) but no frontend to interact with it.
- Problem statement: Users need a UI to manage work items and leverage the AI capabilities of the AI API service.
- Constraints: API calls target ai-api (port 5010) not orgs-api; must use existing patterns (react-hook-form, zod, i18n); Stitch designs required for all screens.

## Decisions & Trade-offs
- **Decision:** Created `aiApiFetch` alongside existing `apiFetch` in `api.ts`.
  - Options considered: Parameterize base URL in existing `apiFetch` vs create separate function.
  - Why this choice: Keeps the two API targets explicit and avoids breaking existing callers. Same implementation pattern.

- **Decision:** Used actual backend WorkItemStatus enum values (`Pending`, `Active`, `Deactivated`, `Expired`) instead of the task spec's original values (`New`, `InProgress`, `Done`, `Cancelled`).
  - Why: The backend domain enum is the source of truth. Using incorrect values would cause runtime errors.

- **Decision:** Server component wrappers for detail/edit pages, client components for list/create.
  - Why: Pages with dynamic `[id]` params use server components for Zod param validation and `notFound()`. Pages without params use `'use client'` directly, matching the audit-logs pattern.

- **Decision:** SmartFill uses a `formKey` counter to remount WorkItemForm with new defaultValues.
  - Options considered: Imperative `setValue()` calls vs remount with new key.
  - Why this choice: Cleaner integration — no need to expose form internals. React key remount is idiomatic.

- **Decision:** ParseWorkItemCommandHandler returns raw text as title with 0.0 confidence on AI failure.
  - Why: Graceful degradation — the user still gets their input in the form and can edit manually.

## Implementation Notes

### Backend Changes (AI API)

1. **Search support in ListWorkItems** — Added `Search` parameter to `ListWorkItemsQuery`, updated handler, repository interface, and implementation. Filter applies `Title.Contains(search)`.

2. **ParseWorkItemCommand** — New command + handler in `Application/WorkItems/Commands/ParseWorkItem/`. Uses `IOpenAiService.GenerateTextAsync()` with a structured JSON prompt. Strips markdown code fences from AI response, validates status values, defaults to `Pending` if invalid.

3. **Parse endpoint** — `POST /v1/work-items/parse` with `[Authorize]` on `WorkItemsController`. Accepts `{ "text": "..." }`, returns `{ "title", "description", "status", "confidence" }`.

4. **Tests** — 6 unit tests for ParseWorkItemCommandHandler covering: valid response, markdown fences, invalid status, AI exception, invalid JSON, long text truncation.

### Frontend Changes

5. **API layer** — Added `AI_API_BASE` and `aiApiFetch` to `api.ts` targeting `NEXT_PUBLIC_AI_API_URL` (port 5010).

6. **Schemas** — Added `WorkItemStatusEnum`, `WorkItemSchema`, `WorkItemsResponseSchema`, `ParseWorkItemResponseSchema`, and `createWorkItemFormSchema` to `schemas.ts`.

7. **Hooks** — Created `hooks/work-items.ts` with: `useWorkItems`, `useWorkItem`, `useCreateWorkItem`, `useUpdateWorkItem`, `useDeleteWorkItem`, `useParseWorkItem`.

8. **Components** — Created in `components/work-items/`:
   - `StatusBadge.tsx` — Color-coded status pills (amber/emerald/slate/red)
   - `WorkItemsTable.tsx` — Data table with loading skeleton, empty state, action links
   - `WorkItemForm.tsx` — react-hook-form + zod, shared between create and edit
   - `SmartFill.tsx` — AI textarea + parse button with success/error feedback
   - `WorkItemDetail.tsx` — Detail view with metadata grid and delete dialog
   - `WorkItemEdit.tsx` — Edit view wrapping WorkItemForm with pre-filled values

9. **Pages** — Created 4 routes:
   - `/work-items/page.tsx` — List with debounced search, status filter, pagination, delete confirmation dialog
   - `/work-items/new/page.tsx` — SmartFill + WorkItemForm, redirects after create
   - `/work-items/[id]/page.tsx` — Server wrapper → WorkItemDetail
   - `/work-items/[id]/edit/page.tsx` — Server wrapper → WorkItemEdit

10. **Navigation** — Added "Work Items" link to AppHeader for authenticated users.

11. **i18n** — Full EN and ES translations for all work items strings (title, subtitle, columns, statuses, actions, form labels, validation, delete dialog, smart fill, breadcrumbs, pagination).

### Agent Instructions
12. **Stitch pixel-perfect mandate** — Updated CLAUDE.md, AGENTS.md, and `.github/copilot-instructions.md` with Stitch MCP workflow requiring `GEMINI_3_PRO` model and pixel-perfect implementation.

## Files Changed

### Created
- `services/ai-api/src/SaasTemplate.AiApi.Application/WorkItems/Commands/ParseWorkItem/ParseWorkItemCommand.cs`
- `services/ai-api/src/SaasTemplate.AiApi.Application/WorkItems/Commands/ParseWorkItem/ParseWorkItemCommandHandler.cs`
- `services/ai-api/src/SaasTemplate.AiApi.Api/Dtos/ParseWorkItemRequest.cs`
- `services/ai-api/tests/SaasTemplate.AiApi.UnitTests/WorkItems/Commands/ParseWorkItemCommandHandlerTests.cs`
- `apps/web/src/hooks/work-items.ts`
- `apps/web/src/components/work-items/StatusBadge.tsx`
- `apps/web/src/components/work-items/WorkItemsTable.tsx`
- `apps/web/src/components/work-items/WorkItemForm.tsx`
- `apps/web/src/components/work-items/SmartFill.tsx`
- `apps/web/src/components/work-items/WorkItemDetail.tsx`
- `apps/web/src/components/work-items/WorkItemEdit.tsx`
- `apps/web/src/app/[locale]/work-items/page.tsx`
- `apps/web/src/app/[locale]/work-items/new/page.tsx`
- `apps/web/src/app/[locale]/work-items/[id]/page.tsx`
- `apps/web/src/app/[locale]/work-items/[id]/edit/page.tsx`

### Modified
- `services/ai-api/src/SaasTemplate.AiApi.Application/WorkItems/Queries/ListWorkItems/ListWorkItemsQuery.cs` (added Search param)
- `services/ai-api/src/SaasTemplate.AiApi.Application/WorkItems/Queries/ListWorkItems/ListWorkItemsQueryHandler.cs` (pass search)
- `services/ai-api/src/SaasTemplate.AiApi.Application/WorkItems/Interfaces/IWorkItemReadRepository.cs` (added search param)
- `services/ai-api/src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/WorkItemReadRepository.cs` (added search filter)
- `services/ai-api/src/SaasTemplate.AiApi.Api/Controllers/WorkItemsController.cs` (added parse endpoint)
- `services/ai-api/tests/SaasTemplate.AiApi.UnitTests/WorkItems/Queries/ListWorkItemsQueryHandlerTests.cs` (fixed for search param)
- `apps/web/src/lib/api.ts` (added aiApiFetch)
- `apps/web/src/lib/schemas.ts` (added WorkItem schemas)
- `apps/web/src/components/layout/AppHeader.tsx` (added Work Items nav link)
- `apps/web/messages/en.json` (added workItems i18n)
- `apps/web/messages/es.json` (added workItems i18n)
- `CLAUDE.md` (Stitch pixel-perfect mandate)
- `AGENTS.md` (Stitch pixel-perfect mandate)
- `.github/copilot-instructions.md` (Stitch pixel-perfect mandate)

## Verification
- `dotnet build services/ai-api/SaasTemplate.AiApi.sln` — 0 errors
- `dotnet test services/ai-api/SaasTemplate.AiApi.sln` — 169 tests passed (88 unit + 76 integration + 5 architecture)
- `npm run build` (apps/web) — 0 errors, all work-items routes generated
- `npm test` (apps/web) — 326 tests passed

## Stitch Designs
- Work Items Management Dashboard: `c9353fe1a99f44e7ae7f651aeeea8427`
- Create Work Item Page: `f8556c4facc0496689579f757b28faa6`
- Work Item Detail View: `63d7f109ec41438eb4d99a21ac4b9310`
- Edit Work Item Page: `0900519ae8974707ab96929cbf13a00d`
