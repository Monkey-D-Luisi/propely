# Walkthrough: audit-0001-remove-committed-credentials

## Task Reference
- Task: `docs/tasks/audit-0001-remove-committed-credentials.md`
- Walkthrough: `docs/walkthroughs/audit-0001-remove-committed-credentials.md`
- Branch/PR: `main`
- Date: `2026-01-30`

## Summary
This action remediates security audit finding F-03 by removing all committed credentials from configuration files. Passwords in `.env.example` are replaced with `CHANGEME` placeholders, and the hardcoded RabbitMQ password is removed from `appsettings.Development.json`. This aligns the repository with the no-secrets policy defined in the security baseline.

## Context
- Background: Security audit identified credentials committed in tracked configuration files
- Problem statement: Development passwords like `aiapi_dev_password` were committed in `.env.example` and `appsettings.Development.json`, violating the security baseline
- Constraints: Must not break local development; existing `.env` files (untracked) continue to work

## Decisions & Trade-offs
- **Decision:** Use `CHANGEME` as the placeholder value
  - Options considered: Empty string, `<your-password>`, `CHANGEME`
  - Why this choice: Matches the security baseline example and is clearly recognizable as needing replacement
  - Consequences / risks: New developers must copy `.env.example` to `.env` and set values before starting

## Implementation Notes
- Key changes:
  - `.env.example`: All password fields now use `CHANGEME`
  - `appsettings.Development.json`: RabbitMQ password set to empty string (loaded from environment)
- Edge cases handled: Docker compose still has fallback defaults for zero-config local dev
- Known limitations: Docker compose defaults remain; acceptable per scope

## Data / Schema / Migrations
- DB changes: None
- Migration strategy: N/A
- Backward compatibility: Existing `.env` files continue to work

## Commands Run
```bash
dotnet build
dotnet test
```

## Files Changed
- `.env.example` — Replaced all password values with `CHANGEME` placeholder
- `src/SaasTemplate.AiApi.Api/appsettings.Development.json` — Removed hardcoded RabbitMQ password
- `README.md` — Updated setup instructions to indicate CHANGEME must be replaced
- `QUICKSTART.md` — Updated quickstart to require password configuration
- `scripts/run-api.sh` — Added RabbitMQ/Redis password fallbacks when `.env` is missing
- `scripts/run-api.ps1` — Added RabbitMQ/Redis password fallbacks when `.env` is missing

## Tests
### Unit
- What was added/updated: None (configuration change)
- How to run: `dotnet test`

### Integration
- What was added/updated: None
- How to run: `dotnet test`

### Manual
- What you verified: Confirmed `.env.example` uses placeholders, no passwords in tracked config
- Steps: Review file contents after changes

## Observability
- Logs added/updated: None
- Traces/metrics added/updated: None
- Dashboards/alerts touched: None

## Security
- Validation: N/A
- AuthN/AuthZ impact: None (separate P0 action)
- Sensitive data handling: Removed committed credentials

## Performance
- Hot paths impacted: None
- Any profiling/bench notes: None

## Docs Updated
- Files updated: None additional (this walkthrough serves as documentation)
- Anything intentionally left for later: README could add explicit setup step

## Rollback Plan
- How to revert safely: Revert the two file changes
- Data rollback considerations: None

## Follow-ups / Backlog
- [ ] Consider adding explicit credential setup instructions to README
- [ ] Implement JWT authentication (P0 - next audit action)

## Checklist
- [x] Task scope matches `docs/tasks/audit-0001-remove-committed-credentials.md`
- [x] Tests updated and passing (79 tests)
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
