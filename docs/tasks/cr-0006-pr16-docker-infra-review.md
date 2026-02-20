# Task: cr-0006-pr16-docker-infra-review

## Metadata
- ID: cr-0006
- Type: CodeReview
- Status: DOING
- PR: #16 — `feat(infra): Docker Compose & infrastructure updates for all 6 services (#0005)`
- Branch: `feat/0005-docker-compose-infrastructure-updates` → `main`
- CI Status: All checks PASSED (Detect Changes, License Headers, Third-Party Notices)
- Owner: Agent
- Created: 2026-02-20

## Changed Files
- `.env.docker` (+32)
- `.env.example` (+113, -1)
- `docker-compose.yml` (+148)
- `docs/backlog/epic-P0-foundation.md` (+2, -2)
- `docs/tasks/0005-docker-compose-infrastructure-updates.md` (+108)
- `docs/walkthroughs/0005-docker-compose-infrastructure-updates.md` (+111)
- `infra/postgres/init/00-init-databases.sql` (+24)
- `scripts/dev-up.ps1` (+14, -5)
- `scripts/dev-up.sh` (+14, -5)
- `scripts/run-appointments-api.ps1` (+30)
- `scripts/run-appointments-api.sh` (+24)
- `scripts/run-contacts-api.ps1` (+30)
- `scripts/run-contacts-api.sh` (+24)
- `scripts/run-properties-api.ps1` (+30)
- `scripts/run-properties-api.sh` (+24)
- `scripts/run-publishing-api.ps1` (+30)
- `scripts/run-publishing-api.sh` (+24)

---

## Section 1: Agent Review Findings

### A.2.1–A.2.2: Architecture, Domain Design
N/A — infrastructure-only PR, no .NET code changes.

### A.2.3: API & Security
- All ports bind to `127.0.0.1` only — PASS
- No secrets in committed files — PASS (dev passwords in .env.example, .env.docker consistent with existing pattern)
- `.env.docker` contains dev-only connection strings with hardcoded dev password — consistent with existing ai-api/orgs-api entries — PASS

### A.2.4: Data & Persistence
- PostgreSQL init script creates 6 databases with pgcrypto + uuid-ossp — PASS
- Init script only runs on fresh volume — documented correctly — PASS

### A.2.5–A.2.7: Testing, Frontend, Inter-Service
N/A — no code changes, no UI changes, no SDK changes.

### A.2.8: Code Quality
- Scripts follow established ai-api pattern — PASS
- PascalCase in env vars matches .NET conventions — PASS
- No dead code, no TODO/HACK/FIXME — PASS

### A.3: Behavioral Parity Checks
- No auth entry points changed — N/A
- No API/UI contract changes — N/A
- No behavior changes requiring tests — N/A

### Agent-Originated Findings

No additional issues found beyond what reviewers identified.

---

## Section 2: Review Comment Threads

### Source 1: Inline Review Comments (9 total)

| # | Reviewer | File | Line | Summary |
|---|----------|------|------|---------|
| 1 | gemini-code-assist | .env.docker | 29 | Missing port in section comment for properties-api |
| 2 | gemini-code-assist | .env.docker | 37 | Missing port in section comment for publishing-api |
| 3 | gemini-code-assist | .env.docker | 45 | Missing port in section comment for contacts-api |
| 4 | gemini-code-assist | .env.docker | 53 | Missing port in section comment for appointments-api |
| 5 | gemini-code-assist | .env.example | 20 | Says "six databases" but should be "seven" (includes default `propely`) |
| 6 | gemini-code-assist | docs/tasks/...md | 93 | "minimum required privileges" wording inaccurate |
| 7 | gemini-code-assist | docs/walkthroughs/...md | 101 | "minimum required privileges" wording inaccurate |
| 8 | gemini-code-assist | infra/postgres/init/...sql | 7 | Suggests explicit least-privilege grants |
| 9 | chatgpt-codex-connector | scripts/run-properties-api.sh | 19 | `source` breaks semicolons in connection strings |

### Source 2: General Reviews (3 total)
- gemini-code-assist: Positive review, no additional issues
- copilot-pull-request-reviewer: 17/17 files reviewed, no comments generated
- chatgpt-codex-connector: Summary header only, issues in inline comments

### Source 3: Issue Comments (1 total)
- gemini-code-assist: PR summary, no action items

---

## Resolution Plan

### MUST_FIX
- [x] **F1** (Codex #9): Fix bash `run-*-api.sh` scripts — `source` breaks semicolons in connection strings. Replace `source` with safe `while read` + `export` loop for all 4 new scripts.

### SHOULD_FIX
- [x] **F2** (Gemini #5): `.env.example` comment says "six databases" but there are 7 (including default `propely`). Update to "seven databases".

### SUGGESTION
- [x] **F3** (Gemini #1–4): Add port numbers to `.env.docker` section comments for consistency with `.env.example`.
- [x] **F4** (Gemini #6–7): Fix "minimum required privileges" wording in task doc and walkthrough. The init script relies on default superuser privileges, not explicit least-privilege grants.

### OUT_OF_SCOPE
- **F5** (Gemini #8): Implement least-privilege PostgreSQL grants. This would require restructuring the init script pattern that is shared with ai-api and orgs-api (pre-existing design). Deferred to a future security hardening task.

### FALSE_POSITIVE
None.

### Follow-ups
- [ ] Pre-existing: `run-ai-api.sh` and `run-orgs-api.sh` have the same `source` semicolon bug (not introduced by this PR)
- [ ] Future task: PostgreSQL init script should use least-privilege grants per service
