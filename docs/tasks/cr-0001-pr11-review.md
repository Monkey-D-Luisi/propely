# CR-0001: Code Review -- PR #11 (docs/propely-foundation)

## PR Metadata
- **PR**: https://github.com/Monkey-D-Luisi/propely/pull/11
- **Branch**: `docs/propely-foundation` → `main`
- **CI Status**: Billing-blocked (infra-ci, claude-review); CI core checks passed (Detect Changes, License Headers, Third-Party Notices)

## Changed Files
524 files changed (+8,407 / -35,865): documentation overhaul, CI runner label config, agent workflow rewrite, skills, terraform version bump.

---

## Section 1: Agent Review Findings

### SHOULD_FIX

1. **`pr-workflow.md:43`** — Affected areas list shows only 3 services `(ai-api, orgs-api, web)` instead of 7.
   - **Fix**: Update to include all 7 services.

2. **`pr-workflow.md:77-82`** — Testing checklist only has ai-api and orgs-api. Missing 4 new services. Also uses `SaasTemplate.*.sln` while the actual PR template (`.github/PULL_REQUEST_TEMPLATE.md`) already uses `Propely.*.sln`.
   - **Fix**: Sync pr-workflow.md testing checklist with the actual PR template.

### NIT

3. **`claude-code-review.yml:34`, `claude.yml:33`** — `actions/checkout@v4` while all other workflows use `@v6`.
   - **Fix**: Update to `@v6`.

### FALSE_POSITIVE (documented rationale)

4. **`ci.yml` SaasTemplate.*.sln references** — CI correctly uses actual on-disk filenames. Rename happens in P0 Task 0.2.
5. **`deploy.yml`, `publish.yml`, `rollback.yml` Docker image prefix `saas-template-`** — CI correctly uses actual Docker image names. Rename happens in P0 Task 0.2.
6. **`ci.yml` change detection only covers 3 services** — Correct; the other 4 services don't exist on disk yet. Will be added when scaffolded.

---

## Section 2: Review Comment Threads

### Gemini Code Assist (inline, 4 comments)

| # | File | Severity | Classification | Summary |
|---|------|----------|---------------|---------|
| G1 | `README.md:47` | High | OUT_OF_SCOPE | Add DB migration step to Quick Start for all 6 services |
| G2 | `.agent.md:53` | Medium | SUGGESTION | Use `####` headings instead of bold for sub-sections |
| G3 | `epic-P1:78` | Medium | OUT_OF_SCOPE | Clarify agency-level role storage (design question for P1 implementation) |
| G4 | `QUICKSTART.md:61` | Medium | OUT_OF_SCOPE | Add migration commands for 4 new services |

### Rationale for OUT_OF_SCOPE items

- **G1, G4**: The 4 new services don't exist on disk. Adding migration commands for non-existent projects would be misleading and fail. When P0 Task 0.3 scaffolds the services, docs will be updated.
- **G3**: This is a design question about `AgencyMembership` vs reusing `Membership`. The epic is a specification; implementation details will be resolved when Task 1.1 is executed. The concern is valid and noted but not actionable in this docs PR.

---

## Resolution Plan

### SHOULD_FIX (Agent)
- [x] A1: Update `pr-workflow.md:43` services list to 7 services
- [x] A2: Sync `pr-workflow.md` testing checklist with `.github/PULL_REQUEST_TEMPLATE.md`

### NIT (Agent)
- [x] A3: Update `actions/checkout` from v4 to v6 in `claude-code-review.yml` and `claude.yml`

### SUGGESTION (Reviewer G2)
- [x] G2: Use `####` headings in `.agent.md` for Task Document Preservation, Scope Discipline, Manual Verification

### OUT_OF_SCOPE (Reviewer G1, G3, G4)
- [x] G1: Replied with rationale (services don't exist yet)
- [x] G3: Replied with rationale (design question for P1 execution)
- [x] G4: Replied with rationale (same as G1)
