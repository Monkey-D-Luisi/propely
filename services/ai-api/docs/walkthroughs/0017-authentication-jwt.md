# Walkthrough: 0017-authentication-jwt

## Task Reference
- Task: `docs/tasks/0017-authentication-jwt.md`
- Walkthrough: `docs/walkthroughs/0017-authentication-jwt.md`
- Branch/PR: `feat/task-0017-auth`
- Date: 2026-02-03

## Summary
Secured the API with JWT Authentication and integrated User Identity into the Work Item domain.

## Context
- Background: API was inclusive and insecure.
- Problem statement: Need to protect resources and audit who creates them.
- Constraints: Use local dev tools for simplicity.

## Decisions & Trade-offs
- **Decision:** Use `dotnet user-jwts`.
  - Why this choice: Native support in .NET, manages secrets automatically for dev, no external dependency needed.
  - Consequences: Testing requires generating tokens via CLI.

## Implementation Notes
- Key changes:
- Edge cases handled:
- Known limitations:

## Data / Schema / Migrations
- DB changes: Added `UserId` column to `WorkItems` and `WorkItemsRead`.
- Migration strategy: EF Core migration.

## Files Changed

## Tests

## Docs Updated

## Checklist
- [ ] Task scope matches `docs/tasks/0017-authentication-jwt.md`
- [ ] Tests updated and passing
- [ ] Docs updated where relevant
- [ ] No secrets committed (.env only / templates for examples)
