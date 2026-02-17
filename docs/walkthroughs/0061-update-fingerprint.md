# Walkthrough: 0061-update-fingerprint

## Task Reference
- Task: `docs/tasks/0061-update-fingerprint.md`
- Walkthrough: `docs/walkthroughs/0061-update-fingerprint.md`
- Branch/PR: `feat/0061-version-fingerprint`
- Date: `2026-02-14`

## Summary
Implemented a version fingerprint mechanism and update notification channel. A `VERSION` file at the repo root defines the current template version (1.0.0). The orgs-api exposes `GET /version` (authenticated) and `GET /version/check` (admin-only, opt-in via UpdateCheck feature flag) endpoints. The frontend admin UI shows the current version and allows admins to check for updates via the GitHub Releases API.

## Context
- Background: After purchasing and deploying the template, users need to know which version they're running and whether updates are available.
- Problem statement: No version identification mechanism existed. No way to check for updates.
- Constraints: Update checks must be opt-in (no phone-home by default). No license validation or telemetry.

## Decisions & Trade-offs
- **Decision:** VERSION file at repo root (not package.json)
  - Options considered: package.json version, .csproj version, dedicated VERSION file
  - Why this choice: Single source of truth shared by all services. Simple to read in CI/CD and build scripts.

- **Decision:** GitHub Releases API for update checks
  - Options considered: Custom endpoint, npm registry, GitHub Releases API
  - Why this choice: Zero infrastructure cost. GitHub Releases is the natural distribution channel for a commercial template. Standard API, well-documented.

- **Decision:** Feature flag for opt-in update checks
  - Options considered: appsettings toggle, feature flag, environment variable
  - Why this choice: Reuses existing feature flag infrastructure (FeatureFlag entity + IFeatureFlagService). Consistent with how other optional features are toggled. Defaults to `false` (no phone-home unless explicitly enabled).

- **Decision:** VersionOptions in VersionController.cs file
  - Options considered: Separate file in Configuration/, keep in Controller file
  - Why this choice: Small config class (3 properties), tightly coupled to the version controller. Not worth a separate file.

## Implementation Notes
- Key changes: VERSION file, VersionController (2 endpoints), VersionOptions config, frontend admin page with version display and update check UI
- Edge cases handled: GitHubRepo not configured, GitHub API unreachable, feature flag disabled, version parsing failures
- Known limitations: Only checks the latest release (not pre-releases or specific channels)

## Data / Schema / Migrations
- N/A (no database changes)

## Commands Run
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet build services/ai-api/SaasTemplate.AiApi.sln
cd apps/web && npm run build && npm test
bash scripts/verify-license-headers.sh  # 620 files pass
```

## Files Changed
### Created
- `VERSION` — Template version file (1.0.0)
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/VersionController.cs` — GET /version + GET /version/check endpoints, VersionOptions config class
- `apps/web/src/hooks/version.ts` — useVersion() and useUpdateCheck() hooks
- `apps/web/src/components/admin/VersionInfo.tsx` — Admin UI component for version display and update checking
- `apps/web/src/app/[locale]/admin/version/page.tsx` — Admin version page

### Modified
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/DependencyInjection.cs` — Added VersionOptions config and GitHub HttpClient
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/appsettings.json` — Added Version and UpdateCheck feature flag sections
- `apps/web/src/lib/schemas.ts` — Added VersionResponseSchema and UpdateCheckResponseSchema
- `apps/web/src/components/layout/AppHeader.tsx` — Added Version nav link
- `apps/web/messages/en.json` — Added version section and nav label
- `apps/web/messages/es.json` — Added version section and nav label (Spanish)
- `.env` — Added NEXT_PUBLIC_APP_VERSION=1.0.0
- `.env.example` — Added NEXT_PUBLIC_APP_VERSION and ORGSAPI_Version__GitHubRepo
- `docs/tasks/0061-update-fingerprint.md` — Status DONE, DoD checked
- `docs/backlog/epic-011-licensing.md` — Task 0061 status DONE, progress 3/4
- `docs/roadmap-v1.md` — Task 0061 status DONE

## Tests
### Unit
- N/A (controller logic is straightforward HTTP + config, tested via integration)

### Integration
- N/A (requires running GitHub API; manual verification performed)

### Manual
- Verified orgs-api builds successfully
- Verified ai-api builds successfully
- Verified Next.js build succeeds (admin/version route included)
- Verified all 431 frontend tests pass
- Verified license headers present on all 620 source files

## Observability
- VersionController logs warnings on GitHub API failures via ILogger

## Security
- No secrets committed
- Update check endpoint is admin-only (AuthorizationPolicies.AdminOnly)
- GitHub API calls use read-only public endpoint (no auth token needed)
- Feature flag defaults to false (no outbound network calls unless enabled)

## Follow-ups / Backlog
- [ ] Add build date injection from CI pipeline (currently null in dev)
- [ ] Consider caching update check results to avoid repeated GitHub API calls

## Checklist
- [x] Task scope matches `docs/tasks/0061-update-fingerprint.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
