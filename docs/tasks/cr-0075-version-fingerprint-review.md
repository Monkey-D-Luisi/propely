# CR-0075: Version Fingerprint Review

## PR Metadata
- **PR:** #290
- **Branch:** `feat/0061-version-fingerprint` → `main`
- **Task:** 0061 (Version fingerprint)
- **CI Status:** All checks passing (7/7)

## Changed Files
16 files: VERSION, VersionController.cs, DependencyInjection.cs, appsettings.json, VersionInfo.tsx, version/page.tsx, AppHeader.tsx, version.ts, schemas.ts, en.json, es.json, .env.example, + 4 docs files

## Review Comments
- **Inline review comments:** 10 (4 Gemini, 6 Copilot)
- **Reviews:** 2 (1 Gemini, 1 Copilot)
- **Issue comments:** 2 (1 Codex usage limit, 1 Gemini summary)

## Comment Resolution Plan

### MUST_FIX
- [x] **Gemini #2807694181** — UI shows green "Up to date" when update check actually failed (message present but updateAvailable=false). Fix: differentiate error state (amber) from success state (green) based on `message` field.

### SHOULD_FIX
- [x] **Gemini #2807694178 + Copilot #2807698055** — Move `VersionOptions` to `Configuration/VersionOptions.cs` in proper namespace. Both reviewers flagged same issue.
- [x] **Copilot #2807698063** — Add NOTE comment documenting the AdminOnly policy limitation (JWT lacks role claims, tracked in audit).
- [x] **Gemini #2807694176** — Add comment noting System.Version limitation with SemVer pre-release strings.

### OUT_OF_SCOPE
- [x] **Gemini #2807694173** — AdminOnly policy only requires authentication, not admin role. This is a known, deferred issue tracked in `docs/audits/service-orgs-api-audit.md` items #1, #2, #7. Affects all admin endpoints (AuditLogs, FeatureFlags, Version). Will be fixed when JwtTokenService emits role claims.

### SUGGESTION (declined)
- [x] **Copilot #2807698056** — Missing hook tests (`version.test.ts`). Valid but not required by task DoD. Follow-up item.
- [x] **Copilot #2807698058** — Missing integration tests for VersionController. Valid but blocked by same auth limitation. Follow-up item.
- [x] **Copilot #2807698060** — Missing VersionInfo component tests. Valid but not required by task DoD. Follow-up item.
- [x] **Copilot #2807698059** — Version nav link visible to all users. This is intentional — version info (GET /version) is available to all authenticated users. Only update checks (GET /version/check) are admin-intended. When proper role-based auth is implemented, the nav link can be conditionally shown.

## Parity Verification Checklist
- [x] Redirect parity checked — No redirects in this feature (version page is standalone, no auth flow changes)
- [x] Locale source correctness checked — Uses next-intl `useTranslations()` consistently; locale from route
- [x] API/UI contract parity checked — VersionResponseSchema matches GET /version response; UpdateCheckResponseSchema matches GET /version/check response; all fields aligned
- [x] Test parity checked — No behavior changes to existing features; new endpoints are read-only informational. Test coverage deferred as noted above
