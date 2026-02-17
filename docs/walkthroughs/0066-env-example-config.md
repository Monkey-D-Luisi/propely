# Walkthrough: 0066-env-example-config

## Task Reference
- Task: `docs/tasks/0066-env-example-config.md`
- Walkthrough: `docs/walkthroughs/0066-env-example-config.md`
- Branch/PR: `feat/0066-env-example-config` / TBD
- Date: `2026-02-14`

## Summary
Rewrote `.env.example` to include all missing environment variables and created `docs/configuration.md` as a comprehensive configuration reference. A buyer can now configure the full stack (auth, billing, email, OAuth, AI) using only these two files — no guessing or reverse-engineering `appsettings.json`.

## Context
- Background: The pre-sale audit (Epic 013) identified `.env.example` as the #1 friction point — it was missing critical variables for JWT, CSRF, Stripe billing, SendGrid email, and email provider selection.
- Problem statement: A buyer could not configure billing, email provider, or production auth without reading `appsettings.json` source code.
- Constraints: Must preserve out-of-the-box dev experience (no required edits for local development). Must not change how .NET reads configuration.

## Decisions & Trade-offs
- **Decision:** Production-only variables are commented out with `#` prefix
  - Options considered: (a) All variables uncommented with empty/dev values, (b) Production-only variables commented out
  - Why this choice: Commented-out variables make it clear they are optional for dev while still documenting their existence. A buyer copies the file and it works immediately for `docker compose up` without any edits.
  - Consequences / risks: Buyer must uncomment variables for production. This is clearly documented in the "Development vs Production" section of `docs/configuration.md`.

- **Decision:** Excluded `Jwt:Authority` from ai-api variables
  - Options considered: (a) Include it, (b) Exclude it
  - Why this choice: The `Jwt:Authority` key exists in `appsettings.json` but is never read by any code — it is dead configuration. Including it would confuse buyers.
  - Consequences / risks: If future code reads this key, `.env.example` will need updating.

- **Decision:** Created a standalone `docs/configuration.md` rather than embedding in README
  - Options considered: (a) Configuration section in README, (b) Standalone docs file, (c) Inline comments only
  - Why this choice: The configuration reference is substantial (~250 lines) and would bloat the README. A standalone file is the standard pattern for SaaS starter kits and can be linked from the Buyer Quickstart guide (task 0056).
  - Consequences / risks: One more file to maintain when adding new configuration variables.

## Implementation Notes
- Key changes:
  - `.env.example`: Added 25+ missing variables organized into clear sections with inline comments including setup URLs
  - `docs/configuration.md`: Full reference with tables for every variable, grouped by service and purpose, with "Development vs Production" section and production checklist
- Variables added to `.env.example`:
  - `AIAPI_OpenAi__ModelId` (was missing)
  - `AIAPI_Jwt__Secret/Issuer/Audience` (commented, prod-only)
  - `ORGSAPI_Jwt__Secret/Issuer/Audience` (commented, prod-only)
  - `ORGSAPI_Csrf__Secret` (commented, prod-only)
  - `ORGSAPI_Email__Provider` (active, default: smtp)
  - `ORGSAPI_Smtp__Host/Port/From/EnableSsl` (commented, dev defaults via Docker Compose)
  - `ORGSAPI_SendGrid__ApiKey/From/FromName` (commented)
  - `ORGSAPI_Billing__Mode` (active, default: free)
  - `ORGSAPI_Billing__Stripe__SecretKey/PublishableKey/WebhookSecret` (commented)
  - `ORGSAPI_Billing__Plans__0-2__StripePriceId` (commented)
  - Rate limiting example (commented)
- Edge cases handled: Variables with dev defaults in `appsettings.Development.json` are clearly marked as "not needed in dev"
- Known limitations: `docs/configuration.md` must be manually maintained when new configuration is added

## Commands Run
```bash
dotnet build services/ai-api/SaasTemplate.AiApi.sln
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
cd apps/web && npm run build
dotnet test services/ai-api/SaasTemplate.AiApi.sln     # 169 tests passed
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln  # 566 tests passed
cd apps/web && npm test                                  # 431 tests passed
```

## Files Changed
- `.env.example` — rewrote with all missing variables, organized by section with descriptive inline comments
- `docs/configuration.md` — new configuration reference with variable tables, dev vs prod guidance, and production checklist
- `docs/backlog/epic-013-presale-hardening.md` — task 0066 status updated to IN_PROGRESS

## Tests
### Unit
- All existing tests pass (1166 total: 169 ai-api + 566 orgs-api + 431 web)
- No new tests needed — this is a documentation-only change

### Manual
- Verified `.env.example` can be copied to `.env` and used without edits for local development
- Verified all `appsettings.json` configurable sections have corresponding `.env.example` entries

## Security
- Validation: No secrets are committed — all sensitive values are either empty or commented out
- AuthN/AuthZ impact: None — this change only documents existing configuration
- Sensitive data handling: Production secrets (JWT, CSRF, Stripe, SendGrid) are commented out with placeholder values like `sk_test_...`

## Follow-ups
- [ ] Task 0072 (one-command onboarding) will use this `.env.example` for automatic `.env` generation
- [ ] Task 0056 (Buyer Quickstart) will reference `docs/configuration.md` for detailed setup
- [ ] Task 0073 (secret rotation guide) will build on the production checklist in `docs/configuration.md`
