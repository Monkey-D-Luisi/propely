# CR-0078: Developer Cookbook Review

## PR Metadata
- **PR:** #293
- **Branch:** `feat/0057-developer-cookbook` → `main`
- **Task:** 0057 (Developer cookbook / extension recipes)
- **CI Status:** No CI checks configured (documentation-only PR)

## Changed Files
5 files: docs/cookbook.md, docs/walkthroughs/0057-developer-cookbook.md, docs/tasks/0057-developer-cookbook.md, docs/backlog/epic-010-docs-onboarding.md, docs/roadmap-v1.md

## Review Comments
- **Inline review comments:** 9 (5 Gemini, 4 Copilot)
- **Reviews:** 2 (1 Gemini, 1 Copilot)
- **Issue comments:** 2 (1 Codex usage limit, 1 Gemini summary — neither actionable)

## Comment Resolution Plan

### MUST_FIX
- [x] **Copilot #2807781235** — LanguageSwitcher auto-detect claim is incorrect. The cookbook states "LanguageSwitcher component automatically picks up the new locale from routing.locales" but the actual component hardcodes locale labels and toggle logic for only en/es. Fixed to instruct updating the LanguageSwitcher component manually.
- [x] **Copilot #2807781245** — Feature flag defaults source is incorrect. The cookbook says "Add the new flag to the IFeatureFlagDefaults implementation" but the actual `FeatureFlagDefaults` class reads from `IOptions<Dictionary<string, bool>>` which is bound to configuration (appsettings.json / env vars). Fixed to reference configuration.

### SHOULD_FIX
- [x] **Copilot #2807781228** — Invoice entity Create() uses object initializer instead of private constructor with parameters (matching WorkItem pattern). Also uses ArgumentException instead of domain-specific exception. Fixed to match the actual WorkItem pattern with private constructor and custom validation method.
- [x] **Gemini #2807775176** — Hook example `useState([])` inferred as `never[]`. Added explicit `Invoice[]` type annotation with type definition.
- [x] **Gemini #2807775177 + #2807775178** — Scaffold module example uses "Invoicing" which creates awkward "Invoicings" DbSet name. Changed to "Invoice" for both PowerShell and bash examples.
- [x] **Gemini #2807775179 + Copilot #2807781230** — French translation typo: "Se deconnecter" → "Se déconnecter". Fixed.

### SUGGESTION (declined)
- [x] **Gemini #2807775175** — Claims Next.js 16 doesn't exist. INCORRECT — `apps/web/package.json` shows `"next": "16.1.6"`. The cookbook correctly says Next.js 16. Declined.

### NOT_ACTIONABLE
- [x] **Codex usage limit comment** — Automated billing notice.
- [x] **Gemini summary comment** — Automated PR summary.

## Parity Verification Checklist
- [x] Redirect parity checked — N/A (documentation-only, no auth/redirect changes)
- [x] Locale source correctness checked — N/A (no locale or i18n changes)
- [x] API/UI contract parity checked — N/A (no API or UI changes)
- [x] Test parity checked — N/A (no behavior changes)
