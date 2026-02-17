# SaaS Starter Kit - GitHub Copilot Instructions

## Priority Order (Always Follow)
1. Read `.agent.md` first (project governance)
2. Check current task in `docs/tasks/`
3. Look at `docs/backlog/` for next work

## Commands

When the user types these commands, execute the corresponding workflow:

### `next task`
**Action:** Execute the autonomous workflow
1. Read `.agent/rules/autonomous-workflow.md`
2. Find next PENDING task in `docs/backlog/`
3. Create task file in `docs/tasks/NNNN-<slug>.md`
4. Create walkthrough in `docs/walkthroughs/NNNN-<slug>.md`
5. Implement following Clean Architecture layers
6. Run build and test for affected service(s)
7. Update walkthrough with summary

### `code review`
**Action:** Execute code review workflow
1. Read `.agent/rules/code-review-workflow.md`
2. Create `docs/tasks/cr-NNNN-<slug>.md`
3. Fetch PR context using `gh` CLI
4. Classify comments (MUST_FIX, SHOULD_FIX, SUGGESTION, QUESTION, OUT_OF_SCOPE)
5. Create resolution plan
6. Implement fixes

### `fast track: <description>`
**Action:** Execute fast track workflow for meta-work
1. Read `.agent/rules/fast-track-workflow.md`
2. Create `docs/tasks/ft-NNNN-<slug>.md`
3. Create `docs/walkthroughs/ft-NNNN-<slug>.md`
4. Implement the urgent/meta task
5. Commit with `ft-` reference

### `next audit action`
**Action:** Execute the audit action workflow
1. Read `.agent/rules/audit-action-workflow.md`
2. Locate the latest executive summary in `docs/audits/`
3. Create `docs/tasks/audit-####-<slug>.md`
4. Create `docs/walkthroughs/audit-####-<slug>.md`
5. Implement the remediation work (if requested)
6. Run build and test
7. Update walkthrough with summary

### `pr`
**Action:** Create a pull request for the current branch
1. Read `.agent/rules/pr-workflow.md`
2. Verify not on `main` branch
3. Push to remote if needed
4. Create PR using template from `.github/PULL_REQUEST_TEMPLATE.md`
5. Fill ALL sections of the template
6. Report PR URL to user

### `audit services`
**Action:** Execute service-level audit workflow
1. Read `.agent/rules/audit-services-workflow.md`
2. Audit all 3 services: web, ai-api, orgs-api
3. Generate `docs/audits/service-<name>-audit.md` for each
4. Cover clean code, security (OWASP Top 10), performance
5. Generate Prioritized Action Plan per service

### `fix service audits`
**Action:** Execute fix workflow for service audit findings
1. Read `.agent/rules/fix-service-audits-workflow.md`
2. Read all 3 service audit reports from `docs/audits/`
3. Implement all fixes, one commit per service
4. Create a single PR with all commits

### `audit epic <NNN>`
**Action:** Perform a comprehensive audit of a completed epic
1. Read `.agent/rules/audit-epic-workflow.md`
2. Analyze documentation, code, commits, tests, architecture compliance
3. Generate executive summary in `docs/audits/epic-<NNN>-executive-summary.md`

### `batch audit actions <NNN>`
**Action:** Process all remaining audit actions from an epic
1. Read `.agent/rules/batch-audit-actions-workflow.md`
2. Implement all `Not started` actions from the epic's executive summary
3. One commit per action, single PR for all

## Quick Reference Commands
```bash
# Build
dotnet build services/ai-api/SaasTemplate.AiApi.sln
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
cd apps/web && npm run build

# Test
dotnet test services/ai-api/SaasTemplate.AiApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
cd apps/web && npm test

# Infrastructure
.\scripts\dev-up.ps1       # or ./scripts/dev-up.sh
.\scripts\dev-down.ps1     # or ./scripts/dev-down.sh
```

## Project Structure
```
apps/
└── web/                          # Next.js 16 frontend

services/
├── ai-api/                       # .NET 10 AI API
│   ├── src/
│   │   ├── SaasTemplate.AiApi.Api/
│   │   ├── SaasTemplate.AiApi.Application/
│   │   ├── SaasTemplate.AiApi.Domain/
│   │   └── SaasTemplate.AiApi.Infrastructure/
│   └── tests/
└── orgs-api/                     # .NET 10 Orgs API
    ├── src/
    │   ├── SaasTemplate.OrgsApi.Api/
    │   ├── SaasTemplate.OrgsApi.Application/
    │   ├── SaasTemplate.OrgsApi.Domain/
    │   └── SaasTemplate.OrgsApi.Infrastructure/
    └── tests/

docs/
├── backlog/      # Epic files with PENDING tasks
├── tasks/        # Task documentation
├── walkthroughs/ # Implementation walkthroughs
└── audits/       # Audit reports

.agent/
├── rules/        # Workflow and standards definitions
└── templates/    # Document templates
```

## Rules (Always Apply)
1. **English only** in all repository files
2. **Every task needs a matching walkthrough** (same filename)
3. **No secrets** in repository
4. **Prefer file changes** over chat explanations
5. **Priority when conflicts:** correctness > security > simplicity > consistency
6. **When creating a PR, always use the GitHub PR template at `.github/PULL_REQUEST_TEMPLATE.md` and fill all sections.**

## Frontend Design System (mandatory for all UI work)

### Tokens
Design tokens are defined in `apps/web/src/app/globals.css` via Tailwind v4 `@theme`. Use semantic `primary-*` classes — never hardcode `indigo-*` for brand/action colors.

### Color usage
| Purpose | Classes |
|---------|---------|
| Primary button | `bg-primary-600 hover:bg-primary-600/90 text-white shadow-sm active:scale-[0.98]` |
| Secondary button | `border-slate-200 bg-white text-slate-700 hover:bg-slate-50` |
| Focus ring (buttons) | `focus:ring-2 focus:ring-primary-600 focus:ring-offset-2` |
| Focus ring (inputs) | `focus:ring-2 focus:ring-primary-600 focus:border-transparent` |
| Links (auth context) | `text-primary-700 underline hover:text-primary-800` |
| Page background | `bg-surface` (#f6f6f8) — semantic token, do NOT use `bg-slate-50` |
| Neutral text | `text-slate-900` / `text-slate-600` / `text-slate-500` (keep slate) |
| Card borders | `border-slate-200` (keep slate) |
| Input borders | `border-slate-200` (keep slate) |

### Border radius
- Cards/containers: `rounded-xl`
- Inputs/buttons: `rounded-lg`
- Badges/pills: `rounded-full`

### Layout widths
- Auth pages: `max-w-md`
- Dashboard/org list: `max-w-4xl`
- Detail pages (members, settings, billing, profile, admin): `max-w-5xl`

### Stitch MCP (UI design tool — pixel-perfect mandate)
- **Stitch Project ID**: `16786124142182555397`
- **Always use `modelId: GEMINI_3_PRO`** when calling `generate_screen_from_text` or `edit_screens`
- Every new screen MUST have a corresponding Stitch design in the project
- **Pixel-perfect implementation is mandatory.** The Stitch design is the single source of truth for all UI work. Implementation must match the Stitch HTML output exactly — same spacing, colors, typography, layout, and component structure.
- **Workflow for UI tasks:**
  1. Generate (or retrieve existing) the Stitch screen design
  2. Download the Stitch HTML to `.stitch-html/<screen-name>.html` for reference
  3. Extract exact CSS classes, spacing, colors, and typography from the Stitch HTML
  4. Implement the page/component matching the Stitch design pixel-for-pixel
  5. Verify visually (Puppeteer screenshot or manual check) against the Stitch design
- If a Stitch design doesn't exist yet for a screen, generate one before writing any UI code.
- Reference: `docs/walkthroughs/0065-design-system.md`

## Critical Rules for Task Execution

### Task Document Immutability
When completing a task, **only update** the `Status` field and Definition of Done checkboxes in the task file. **NEVER** delete or rewrite Requirements, Acceptance Criteria, Context, Constraints, Scope, Implementation Steps, or Testing Plan. You may add new sections at the end. Implementation details belong in the walkthrough.

### Scope Discipline
Only create, modify, or delete files directly required by the task's scope. Do not add CI/CD workflows, tooling configs, or unrelated improvements. Note out-of-scope ideas in the walkthrough Follow-ups section.

### Manual Verification
If the Testing Plan includes manual steps, complete automated work first, then ask the user to perform manual verification before marking the task DONE.

### Error Handling (Non-Negotiable)
- Never use bare `catch` or `catch (Exception)` without logging
- Every catch block must: (a) catch the most specific exception type, (b) log with context via `ILogger`, (c) handle appropriately
- Bare `catch { }` blocks are prohibited

### Code Quality
- Before committing, verify that no duplicated logic blocks contain unreachable code
- When copy-pasting patterns across files, re-evaluate each guard clause in the new context
- Remove dead code before committing

## Clean Architecture Layer Order
When implementing features, follow this order:
1. **Domain** — Entities, value objects, domain events
2. **Application** — Commands, queries, handlers, interfaces
3. **Infrastructure** — Repositories, external services
4. **Presentation** — Controllers, DTOs, validation

## Task Numbering
- **Regular tasks:** `NNNN-<slug>.md`
- **Code reviews:** `cr-NNNN-<slug>.md`
- **Fast track:** `ft-NNNN-<slug>.md`
- **Audit actions:** `audit-####-<slug>.md`

Scan `docs/tasks/` to find the next sequential number for each type.

## Quality Gates (Before Completing Any Task)
```bash
dotnet build services/ai-api/SaasTemplate.AiApi.sln     # Must pass
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln  # Must pass
dotnet test services/ai-api/SaasTemplate.AiApi.sln       # Must pass
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln   # Must pass
cd apps/web && npm run build                              # Must pass
cd apps/web && npm test                                   # Must pass
```
