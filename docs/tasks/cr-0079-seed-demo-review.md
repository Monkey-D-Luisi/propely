# Code Review: cr-0079-seed-demo-review

## PR Metadata
- PR: #295 `feat(scripts): rich demo seed script with 5 users, 3 orgs, 13 work items (#0063)`
- Branch: `feat/0063-enhanced-seed-data` -> `main`
- CI status: All checks SUCCESS (Detect Changes, License Headers, Third-Party Notices); build/test SKIPPED (no app changes)

## Changed Files
- `scripts/seed-demo.sh` (+465 lines, new)
- `scripts/seed-demo.ps1` (+355 lines, new)
- `docs/walkthroughs/0063-enhanced-seed-data.md` (+106 lines, new)
- `docs/tasks/0063-enhanced-seed-data.md` (+13/-8)
- `docs/backlog/epic-012-demo-marketing.md` (+3/-3)
- `docs/roadmap-v1.md` (+1/-1)

## Review Threads

### Source 1: Inline Review Comments (12 total)

| # | Reviewer | File:Line | Summary | Classification |
|---|----------|-----------|---------|---------------|
| 1 | Gemini | seed-demo.ps1:191 | X-Test-User-Id/X-Test-Org-Id headers bypass auth | OUT_OF_SCOPE |
| 2 | Gemini | seed-demo.sh:204 | X-Test-User-Id/X-Test-Org-Id headers bypass auth | OUT_OF_SCOPE |
| 3 | Gemini | seed-demo.sh:146 | Fallback to "first available org" is flawed | SHOULD_FIX |
| 4 | Gemini | seed-demo.ps1:144 | Fallback to first org + throw breaks resilience | SHOULD_FIX |
| 5 | Gemini | seed-demo.ps1:31 | Hardcoded password — allow env var override | SHOULD_FIX |
| 6 | Gemini | seed-demo.sh:25 | Hardcoded password — allow env var override | SHOULD_FIX |
| 7 | Gemini | seed-demo.sh:39 | Misleading comment "Waits for Retry-After header" | MUST_FIX |
| 8 | Gemini | seed-demo.sh:63 | `grep -oP` not portable on macOS | SUGGESTION |
| 9 | Gemini | seed-demo.sh:99 | Global variables for function return values | SUGGESTION |
| 10 | Copilot | seed-demo.ps1:342 | Inconsistent formatting (hyphen vs em-dash) | SHOULD_FIX |
| 11 | Copilot | seed-demo.ps1:315 | Work item descriptions truncated vs bash | SHOULD_FIX |
| 12 | Copilot | seed-demo.ps1:1 | Missing shebang `#!/usr/bin/env pwsh` | SHOULD_FIX |

### Source 2: General Reviews (2 total)
- Gemini: Summary of concerns (security headers, hardcoded creds, org fallback, portability)
- Copilot: Overview summary, no additional actionable items beyond inline comments

### Source 3: Issue Comments (2 total)
- ChatGPT Codex: Usage limit notice (not actionable)
- Gemini: PR summary (not actionable)

## Behavioral Parity Checks
- [x] Redirect parity checked — N/A, standalone seed scripts with no auth redirects
- [x] Locale source correctness checked — N/A, no localized flows
- [x] API/UI contract parity checked — N/A, no API or UI contract changes
- [x] Test parity checked — N/A, no testable behavior changes (seed scripts tested manually)

## Comment Resolution Plan

### MUST_FIX
- [x] #7: Fix misleading `curl_retry` comment — change "Waits for Retry-After header" to "Waits for a fixed interval"

### SHOULD_FIX
- [x] #3: Remove "first available org" fallback in bash `create_org` — skip directly when org not found by name
- [x] #4: Remove "first available org" fallback in PS1 `New-Org` — return empty instead of throw
- [x] #5: Allow env var override for password in PS1 (`$env:DEMO_PASSWORD`)
- [x] #6: Allow env var override for password in bash (`${DEMO_PASSWORD:-Demo123!}`)
- [x] #10: Use em-dash (—) in PS1 summary output to match bash
- [x] #11: Sync work item descriptions between PS1 and bash (use full descriptions)
- [x] #12: Add `#!/usr/bin/env pwsh` shebang to seed-demo.ps1

### SUGGESTION (not applied — rationale below)
- [x] #8: `grep -oP` portability — Not applied. This is a dev-only script. The existing `seed-dev.sh` uses `grep -oP` throughout, establishing a project convention. The prerequisite comment in the script header mentions "curl and grep available", and this targets Linux/WSL/Docker. macOS users can install GNU grep via `brew install grep`.
- [x] #9: Global variables for return values — Not applied. Refactoring all functions to use stdout return + stderr for messages is disproportionate for a dev seed script. The existing `seed-dev.sh` uses the same global variable pattern, and the script is straightforward to follow.

### OUT_OF_SCOPE (rationale)
- [x] #1, #2: X-Test headers — These headers are only honored when `Security:AllowAnonymous=true` in the AI API configuration, which is a development-only setting. Production environments use JWT-based authentication with this flag disabled. The existing `seed-dev.sh` uses the same mechanism. No change needed in the seed script; the security concern is addressed at the API configuration level.
