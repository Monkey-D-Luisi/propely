# CR-0070: One-Command Onboarding PR Review

## PR Metadata
- **PR:** #285 — feat(dx): one-command developer onboarding (#0072)
- **Branch:** `feat/0072-one-command-onboarding` → `main`
- **CI Status:** Web Build & Test: SUCCESS

## Changed Files
- `scripts/bootstrap.sh` (new)
- `scripts/bootstrap.ps1` (new)
- `Makefile` (new)
- `README.md` (modified)
- `docs/tasks/0072-one-command-onboarding.md` (modified)
- `docs/walkthroughs/0072-one-command-onboarding.md` (new)
- `docs/backlog/epic-013-presale-hardening.md` (modified)
- `docs/roadmap-v1.md` (modified)

## Review Sources
- Inline review comments: 13
- General reviews: 2 (gemini-code-assist, Copilot — summaries only)
- Issue comments: 2 (non-actionable: codex limit msg, gemini summary)

## Comment Resolution Plan

### MUST_FIX

- [x] **`grep -oP` not portable on macOS** (gemini #2807456492, copilot #2807459054)
  - macOS BSD grep lacks `-P` (Perl regex). Replace with portable `sed` for Docker version extraction.

- [x] **Health endpoints `/health` don't exist — should be `/health/live`** (copilot #2807459049 on ps1, copilot #2807459070 on sh)
  - Verified: HealthChecksConfiguration.cs maps `/health/live` (liveness) and `/health/ready` (readiness). No `/health` endpoint exists. Fix both scripts to use `/health/live`.

### SHOULD_FIX

- [x] **`help` missing from `.PHONY`** (gemini #2807456493, copilot #2807459061)
  - Add `help` to `.PHONY` declaration.

- [x] **Docker daemon try/catch simplification** (gemini #2807456496, copilot #2807459056)
  - With `$ErrorActionPreference = "Stop"`, `docker ps` throws before `$LASTEXITCODE` check. Simplify to rely on try/catch cleanly.

- [x] **`$LASTEXITCODE -and $LASTEXITCODE -ne 0` redundancy** (gemini #2807456497)
  - Simplify to `if ($LASTEXITCODE)`.

- [x] **`dotnet --version` empty check** (copilot #2807459067)
  - Add null/empty guard before parsing version string.

- [x] **Explicit dev-up.sh exit code check** (copilot #2807459068)
  - Use `if ./scripts/dev-up.sh; then` pattern for better error messaging (commands in `if` are exempt from `set -e`).

- [x] **Walkthrough health endpoint docs** (copilot #2807459051)
  - Update walkthrough to reference `/health/live` and `/health/ready` correctly.

### OUT_OF_SCOPE

- [ ] **Unicode banner inconsistency with existing scripts** (copilot #2807459058)
  - Rationale: The bootstrap script is intentionally a premium, buyer-facing first impression. Existing `dev-up.sh` is an internal developer tool. Unicode box-drawing characters render correctly on all modern terminals (macOS Terminal, iTerm2, Windows Terminal, GNOME Terminal). The visual distinction between bootstrap and internal scripts is intentional.

## Behavioral Parity Checks

- [x] Redirect parity checked — N/A (no auth flows)
- [x] Locale source correctness checked — N/A (no localized content)
- [x] API/UI contract parity checked — N/A (no API changes)
- [x] Test parity checked — N/A (scripts, not testable behavior)
