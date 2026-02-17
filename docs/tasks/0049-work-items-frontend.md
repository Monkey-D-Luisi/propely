# Task: 0049 - Work Items Frontend (AI API CRUD Pages)

## Metadata
- ID: 0049
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #201
- Epic: `docs/backlog/epic-007-admin-dashboards.md`
- Old Issues: #20, #22
- Milestone: v1.0

## Goal
Create frontend pages for managing work items (the AI API's primary domain entity) with list, create, edit, and detail views.

## Context
The ai-api has a fully functional WorkItems backend: `WorkItemsController` with CRUD endpoints (GET list, GET by ID, POST create, PUT update, DELETE). The backend supports pagination and filtering. What's missing is the frontend to interact with these endpoints.

### Existing Backend Endpoints (ai-api, port 5010)
- `GET /work-items` - list with pagination (page, pageSize, search, status)
- `GET /work-items/{id}` - get by ID
- `POST /work-items` - create (title, description, status)
- `PUT /work-items/{id}` - update (title, description, status)
- `DELETE /work-items/{id}` - soft delete

### WorkItem Properties
- Id, Title, Description, Status (New, InProgress, Done, Cancelled), CreatedAtUtc, UpdatedAtUtc

## Scope
### In scope
- `/work-items` list page with pagination and search
- `/work-items/new` create page with form
- `/work-items/{id}` detail page
- `/work-items/{id}/edit` edit page with form
- Status filtering and search by title
- Delete confirmation dialog
- react-hook-form + zod for forms
- i18n strings (EN + ES)

### Out of scope
- Drag-and-drop kanban board
- Work item comments
- Work item assignments
- AI-powered features (just CRUD)

## Requirements
- R1: Users can list work items with pagination
- R2: Users can search work items by title
- R3: Users can filter by status
- R4: Users can create new work items
- R5: Users can view work item details
- R6: Users can edit work items
- R7: Users can delete work items (with confirmation)

## Acceptance Criteria
- AC1: `/work-items` shows paginated list with search and status filter
- AC2: `/work-items/new` has form with title, description, status fields
- AC3: `/work-items/{id}` shows work item details
- AC4: `/work-items/{id}/edit` has pre-filled edit form
- AC5: Delete button with confirmation dialog soft-deletes the item
- AC6: `npm run build` and `npm test` pass
- AC7: i18n strings in EN + ES

## Constraints (non-negotiable)
- API calls go to ai-api (port 5010), not orgs-api
- Use react-hook-form + zod for forms
- Use existing patterns (apiFetch, hooks, schemas)
- i18n for all strings
- Update walkthrough

## Implementation Steps

1. **Create work items hooks** (`apps/web/src/hooks/work-items.ts`)
   - `useWorkItems(page, search, status)`: GET /work-items from ai-api
   - `useWorkItem(id)`: GET /work-items/{id}
   - `useCreateWorkItem()`: POST /work-items
   - `useUpdateWorkItem()`: PUT /work-items/{id}
   - `useDeleteWorkItem()`: DELETE /work-items/{id}
   - Note: apiFetch needs to target port 5010 (configure NEXT_PUBLIC_AI_API_URL)

2. **Add Zod schemas** (`apps/web/src/lib/schemas.ts`)
   - `WorkItemSchema`, `WorkItemsResponseSchema` (with pagination)
   - `workItemFormSchema` (title required, description optional, status enum)

3. **Create list page** (`apps/web/src/app/[locale]/work-items/page.tsx`)
   - Search input, status filter dropdown, pagination controls
   - Table: Title, Status, Created, Actions (view/edit/delete)

4. **Create WorkItemsTable component** (`apps/web/src/components/work-items/WorkItemsTable.tsx`)

5. **Create new page** (`apps/web/src/app/[locale]/work-items/new/page.tsx`)
   - Form with react-hook-form + zod
   - Title (text input), Description (textarea), Status (select)
   - Submit creates and redirects to detail page

6. **Create detail page** (`apps/web/src/app/[locale]/work-items/[id]/page.tsx`)
   - Display all fields
   - Edit and Delete action buttons

7. **Create edit page** (`apps/web/src/app/[locale]/work-items/[id]/edit/page.tsx`)
   - Pre-filled form with current values
   - Submit updates and redirects to detail page

8. **Create WorkItemForm component** (`apps/web/src/components/work-items/WorkItemForm.tsx`)
   - Shared between create and edit pages
   - react-hook-form + zod validation

9. **Create StatusBadge component** (`apps/web/src/components/work-items/StatusBadge.tsx`)
   - Color-coded badge for each status

10. **Add i18n strings** (`apps/web/messages/en.json`, `apps/web/messages/es.json`)
    - `workItems.title`, `workItems.list`, `workItems.create`, `workItems.edit`, `workItems.delete`, `workItems.status.*`, `workItems.search`, `workItems.noItems`

11. **Add navigation link** to AppHeader or sidebar

### Testing

12. **Frontend tests**
    - WorkItemsTable renders data
    - WorkItemForm validates input
    - Create page submits correctly
    - Delete confirmation dialog

## Files to Create / Modify

### Create
- `apps/web/src/app/[locale]/work-items/page.tsx`
- `apps/web/src/app/[locale]/work-items/new/page.tsx`
- `apps/web/src/app/[locale]/work-items/[id]/page.tsx`
- `apps/web/src/app/[locale]/work-items/[id]/edit/page.tsx`
- `apps/web/src/hooks/work-items.ts`
- `apps/web/src/components/work-items/WorkItemsTable.tsx`
- `apps/web/src/components/work-items/WorkItemForm.tsx`
- `apps/web/src/components/work-items/StatusBadge.tsx`
- `docs/walkthroughs/0049-work-items-frontend.md`

### Modify
- `apps/web/src/lib/schemas.ts`
- `apps/web/src/components/layout/AppHeader.tsx`
- `apps/web/messages/en.json`
- `apps/web/messages/es.json`

## Testing Plan
- Frontend tests: All components and hooks
- Manual: Full CRUD flow against running ai-api

## Definition of Done Checklist
- [x] Acceptance criteria met (including AC8-AC11 from scope extension)
- [x] Build passes
- [x] Tests added/updated and pass
- [x] i18n strings added (EN + ES)
- [x] Formatting/analyzers pass
- [x] Walkthrough updated

---

## Scope Extension: AI Smart Input Parsing (v1.0 replanning)

> Added during v1.0 replanning. The AI API's `IOpenAiService` is registered but unused.
> This extension integrates it into the work items UI via a smart input parsing feature.
> See `docs/roadmap-v1.md` for full strategic context.

### Extended Goal
In addition to the CRUD frontend described above, integrate AI-powered smart input parsing:
users type natural language and the AI extracts structured WorkItem fields (Title, Description, Status).

### AI Context
The ai-api has `IOpenAiService` (registered but unused) providing `GenerateTextAsync(prompt, ct)`
using the OpenAI SDK (`gpt-5-mini`). A new `POST /v1/work-items/parse` endpoint will accept
natural language text and return structured work item fields.

### Additional In-Scope Items
- AI smart input parsing: "Smart Fill" textarea on create form that sends natural language to `POST /v1/work-items/parse` and pre-fills form fields
- Backend: new `ParseWorkItemCommand` + handler using `IOpenAiService`
- The original "Out of scope: AI-powered features" is superseded by this extension

### Additional Out-of-Scope Items
- Extending WorkItem domain model (Priority, DueDate, Tags — future iteration)

### Additional Requirements
- R8: Users can type natural language and auto-fill the create form via AI parsing
- R9: AI parsing returns Title, Description, and Status from free-form text

### Additional Acceptance Criteria
- AC8: Create form has "Smart Fill" textarea + button that calls AI parse endpoint
- AC9: AI-parsed fields pre-fill the form; user can review/edit before submitting
- AC10: `POST /v1/work-items/parse` endpoint works with `IOpenAiService`
- AC11: AI parsing gracefully degrades if AI API is unavailable

### Additional Implementation Steps

#### Backend (AI API — smart input parsing)

13. **Create ParseWorkItemCommand + handler** (`services/ai-api/src/SaasTemplate.AiApi.Application/WorkItems/Commands/ParseWorkItem/`)
    - `ParseWorkItemCommand(string Text)` → `ParseWorkItemResult(string Title, string? Description, string Status, double Confidence)`
    - Handler injects `IOpenAiService`, builds structured prompt with JSON schema instructions
    - Returns parsed fields without creating the work item (user confirms first)

14. **Add parse endpoint** to `WorkItemsController`
    - `POST /v1/work-items/parse` with `[Authorize]`
    - Request: `{ "text": "..." }` → Response: `{ "title": "...", "description": "...", "status": "Pending", "confidence": 0.85 }`

15. **Add backend tests** for ParseWorkItemCommandHandler

#### Frontend (AI parsing integration)

16. **Add `useParseWorkItem()` hook** to `apps/web/src/hooks/work-items.ts`
    - `POST /work-items/parse` (AI smart fill)

17. **Create SmartFill component** (`apps/web/src/components/work-items/SmartFill.tsx`)
    - Textarea for natural language input + "Smart Fill" button
    - Calls `useParseWorkItem()` hook
    - On success, pre-fills parent form fields via callback
    - Loading state while AI processes
    - Error/fallback state if AI API is unavailable
    - Only shown on the create page (not edit)

### Additional Files

#### Create (Backend — AI API)
- `services/ai-api/src/SaasTemplate.AiApi.Application/WorkItems/Commands/ParseWorkItem/ParseWorkItemCommand.cs`
- `services/ai-api/src/SaasTemplate.AiApi.Application/WorkItems/Commands/ParseWorkItem/ParseWorkItemCommandHandler.cs`
- `services/ai-api/tests/.../WorkItems/Commands/ParseWorkItemCommandHandlerTests.cs`

#### Create (Frontend)
- `apps/web/src/components/work-items/SmartFill.tsx`

#### Modify (Backend — AI API)
- `services/ai-api/src/SaasTemplate.AiApi.Api/Controllers/WorkItemsController.cs` (add parse endpoint)

### Additional Testing
- Backend tests: ParseWorkItemCommandHandler (mocked IOpenAiService)
- Frontend tests: SmartFill component
- Manual: AI parsing flow end-to-end
