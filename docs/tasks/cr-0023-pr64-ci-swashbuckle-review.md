# CR-0023: Code Review - PR #64 CI Swashbuckle & Loading Skeletons

## PR Metadata
- **PR**: [#64](https://github.com/Monkey-D-Luisi/propely/pull/64)
- **Branch**: `fix/ci-swashbuckle-and-audit-loading` -> `main`
- **Changed files**: 48
- **Reviewers**: Copilot, Gemini Code Assist

## Changed Files
- `.agent/rules/autonomous-workflow.md` — visual validation step
- `.agent/rules/testing-standards.md` — E2E + MCP docs
- `.github/dependabot.yml` — grouping + PR limits
- `.github/workflows/ci.yml` — change detection, E2E fixes, optimizations
- `.third-party-notices-hash` — updated hash
- `CLAUDE.md` — visual validation tools docs
- `apps/web/vitest.config.ts` — loading.tsx coverage exclusion
- `apps/web/src/app/[locale]/**/loading.tsx` — 25 skeleton files
- `docker-compose.ci.yml` — production Dockerfiles, port mappings, healthchecks
- `scripts/generate-third-party-notices.sh` — hash scope fix
- `services/*/Dockerfile` — krb5-libs for Alpine
- `services/*-api/src/*/DependencyInjection.cs` — Swashbuckle v10 migration (4 services)
- `services/*-api/*.csproj` — Swashbuckle 6.6.2 → 10.1.4 (4 services)

---

## Section 1: Agent Review Findings

### A.2.1 Architecture & Clean Architecture
- No violations. Swashbuckle migration is API layer only, dependencies flow correctly.

### A.2.3 API & Security
- `InferSecuritySchemes()` correctly replaces manual security definitions — equivalent behavior with less code.

### A.2.6 Frontend
- **SHOULD_FIX**: Loading skeletons use hardcoded `max-w-[1200px]`/`max-w-[1440px]`/`max-w-[960px]` instead of design system tokens `max-w-4xl`/`max-w-5xl`.
- **SHOULD_FIX**: Several agency/billing/permissions cards use `rounded-lg` instead of `rounded-xl` for containers.

### A.2.8 Code Quality
- **SHOULD_FIX**: `generate-third-party-notices.sh` line 214 has a very long line with duplicated path lists. Should use array variable.

### A.3 Behavioral Parity
- No issues. Changes are additive (loading states) or infrastructure fixes.

---

## Section 2: Review Comment Threads

### Source 1: Inline Comments (26)

| # | Reviewer | File | Classification | Description |
|---|----------|------|----------------|-------------|
| 1 | Copilot | `scripts/generate-third-party-notices.sh:214` | SHOULD_FIX | Refactor DEP_HASHES path list into variable/array |
| 2 | Gemini | `scripts/generate-third-party-notices.sh:214` | SHOULD_FIX | Same as #1 — refactor duplicated path list |
| 3-5 | Gemini | `admin/*/loading.tsx` (3 files) | SHOULD_FIX | `max-w-[1440px]`/`max-w-[1200px]` → `max-w-5xl` |
| 6-8 | Gemini | `agencies/*/loading.tsx` (3 cards) | SHOULD_FIX | `rounded-lg` → `rounded-xl` for cards |
| 9-11 | Gemini | `orgs/billing/loading.tsx` (2 cards) + `permissions/[userId]` (1 card) | SHOULD_FIX | `rounded-lg` → `rounded-xl` for cards |
| 12 | Gemini | `profile/loading.tsx` | SHOULD_FIX | `max-w-[960px]` → `max-w-5xl` |
| 13 | Gemini | `verify-email/loading.tsx` | SHOULD_FIX | `max-w-lg` → `max-w-md` |
| 14-26 | Gemini | Various dashboard `loading.tsx` (13 files) | SHOULD_FIX | `max-w-[1200px]` → `max-w-4xl`/`max-w-5xl` |

### Source 2: General Reviews (2)
- **Copilot**: Summary review; suppressed low-confidence comment about missing walkthrough file.
- **Gemini**: Summary review noting style guide violations in loading skeletons.

### Source 3: Issue Comments (2)
- No actionable items (bot summary comments).

---

## Resolution Plan

### SHOULD_FIX (all applied)
- [x] Refactor `generate-third-party-notices.sh` DEP_HASHES into `SERVICE_SRC_DIRS` array
- [x] Fix `max-w-[1200px]`/`max-w-[1440px]`/`max-w-[960px]`/`max-w-lg` → design system tokens (17 files)
- [x] Fix `rounded-lg` → `rounded-xl` for card containers (7 elements across 5 files)

### FALSE_POSITIVE
- None

### OUT_OF_SCOPE
- Copilot (suppressed): Missing walkthrough file — addressed by this cr-0023 task/walkthrough pair
