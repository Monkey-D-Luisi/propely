# Walkthrough: 0011-agency-management-ui

## Task Reference
- Task: `docs/tasks/0011-agency-management-ui.md`
- Walkthrough: `docs/walkthroughs/0011-agency-management-ui.md`
- Branch/PR: `feat/agency-management-ui`
- Date: `2026-03-05`

## Summary
Build the frontend screens for agency management in the Next.js web app. This includes agency creation, dashboard with branches, branch switcher in the header, and agency settings page.

## Context
- Background: The backend agency API endpoints exist (create, list, get, add/remove branch). The frontend needs UI for agency management.
- Problem statement: Users need to create and manage agencies, view branches, and switch between them.
- Constraints: Follow existing patterns from orgs pages, use established design system.

## Decisions & Trade-offs
- **Decision:** Follow existing hook patterns from `hooks/orgs.ts`
  - Options considered: React Query vs custom hooks
  - Why this choice: Consistency with existing codebase
  - Consequences: Manual state management but familiar pattern

- **Decision:** Use existing page routing pattern (`/agencies/[id]`)
  - Options considered: Nested under orgs vs standalone
  - Why this choice: Agencies are a higher-level concept than orgs
  - Consequences: New top-level route

## Implementation Notes
- Key changes: (to be filled after implementation)
- Edge cases handled: (to be filled)
- Known limitations: Update/delete agency endpoints not yet available in backend

## Data / Schema / Migrations
- DB changes: None (frontend-only)
- Migration strategy: N/A
- Backward compatibility: N/A

## Commands Run
```bash
# To be filled during implementation
```

## Files Changed
- (to be filled after implementation)

## Tests
### Unit
- What was added/updated: Component tests for all new agency components
- How to run: `cd apps/web && npx vitest run`

### Integration
- N/A

### Manual
- What you verified: (to be filled)
- Steps: (to be filled)

## Observability
- Logs added/updated: None
- Traces/metrics added/updated: None

## Security
- Validation: Form inputs validated with Zod
- AuthN/AuthZ impact: None (uses existing auth)
- Sensitive data handling: None

## Follow-ups / Backlog
- [ ] Add update agency API endpoint (PATCH /api/agencies/{id})
- [ ] Add delete agency API endpoint (DELETE /api/agencies/{id})
- [ ] Stitch MCP designs for agency screens

## Checklist
- [ ] Task scope matches `docs/tasks/0011-agency-management-ui.md`
- [ ] Tests updated and passing
- [ ] Docs updated where relevant
- [ ] No secrets committed (.env only / templates for examples)
