# Task: 0061 - Update Channel / Version Fingerprint

## Metadata
- ID: 0061
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #213
- Epic: `docs/backlog/epic-011-licensing.md`
- Old Issues: #48, #49
- Milestone: v1.0

## Goal
Implement a version fingerprint mechanism and update notification channel so template users know when new versions are available.

## Context
After users purchase and deploy the template, they need a way to check for updates. A version fingerprint embedded in the template identifies the installed version, and an update channel provides notifications about new releases.

## Scope
### In scope
- `VERSION` file or constant with current template version
- Build-time version injection (Git tag or package.json version)
- `/api/version` endpoint returning current version info
- Update check mechanism (poll a known endpoint or GitHub releases API)
- Admin UI notification for available updates
- Opt-in/opt-out for update checks

### Out of scope
- Automatic updates / hot patching
- License validation against a server
- Usage telemetry

## Requirements
- R1: Current template version is identifiable
- R2: Version check is opt-in (not mandatory)
- R3: Available updates are shown to admins
- R4: No phone-home if user opts out

## Acceptance Criteria
- AC1: `/api/version` returns current template version
- AC2: Admin page shows current version and update availability
- AC3: Update check can be disabled via config

## Implementation Steps

1. **Create VERSION file** in root or use package.json version
2. **Inject version at build time** into both .NET and Next.js apps
3. **Create version endpoint** in orgs-api
4. **Create update check service** (call GitHub releases API)
5. **Add update notification** to admin UI

## Files to Create / Modify

### Create
- `VERSION` (or use existing package.json)
- `docs/walkthroughs/0061-update-fingerprint.md`

### Modify
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/` (add version endpoint)
- `apps/web/src/components/admin/` (add update notification)

## Definition of Done Checklist
- [x] Version identifiable
- [x] Update check works
- [x] Opt-in/opt-out configurable
- [x] Walkthrough updated
