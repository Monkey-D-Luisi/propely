# Task: cr-0008 — PR #18 Agency Entity Review

## PR Metadata
- PR: #18 (`feat/0007-agency-entity-hierarchy`)
- Base: main
- CI: Orgs API integration tests FAILING (6 tests), other services unrelated NuGet vuln failures
- Reviews: Copilot (12 comments), Gemini (7 comments)

## Changed Files
63 files changed — see PR description for full list.

---

## Section 1: Agent Review Findings

### F1: MUST_FIX — Hardcoded role validators not updated
- **File:** `Api/Validators/InviteRequestValidator.cs:11`, `Api/Validators/UpdateRoleRequestValidator.cs:11`
- **Severity:** MUST_FIX
- **Category:** Code Quality / Correctness
- **Description:** Hardcoded `ValidRoles` arrays still contain `"member"` instead of `"agent"`. This causes all integration tests that invite with role `"Agent"` to get 400 Bad Request.
- **Fix:** Update arrays and error messages. Additionally, derive the list from `Enum.GetNames<MembershipRole>()` to prevent future drift.

### F2: MUST_FIX — LoadBranchIdsAsync uses AddBranch domain method for hydration
- **File:** `Infrastructure/Persistence/Repositories/AgencyRepository.cs:92-106`
- **Severity:** MUST_FIX
- **Category:** DDD / Architecture
- **Description:** Using `agency.AddBranch(branchId)` to load existing branches from DB is incorrect. It raises domain events and then clears them, mixing loading with domain behavior. If `AddBranch` logic changes, this will break silently.
- **Fix:** Add a `HydrateBranches(IEnumerable<Guid>)` internal method to `Agency` that directly populates `_branchIds` without raising events.

### F3: MUST_FIX — Missing authorization check in AddBranchToAgencyCommandHandler
- **File:** `Application/Agencies/Commands/AddBranchToAgency/AddBranchToAgencyCommandHandler.cs:37`
- **Severity:** MUST_FIX
- **Category:** Security
- **Description:** Only checks that the requesting user owns the agency. Does NOT verify they have Owner/Admin rights on the target organization. An agency owner could link ANY organization (even ones they don't own) to their agency.
- **Fix:** Add membership check: requesting user must be Owner or Admin of the target organization.

### F4: SHOULD_FIX — N+1 query in ListForUserAsync
- **File:** `Infrastructure/Persistence/Repositories/AgencyRepository.cs:78-83`
- **Severity:** SHOULD_FIX
- **Category:** Performance
- **Description:** Iterates over agencies and calls `LoadBranchIdsAsync` per agency, resulting in N+1 queries. Should batch-load all branch IDs in a single query.
- **Fix:** Collect all agency IDs, query all matching organizations in one batch, group by AgencyId in memory.

### F5: SHOULD_FIX — Missing unit tests for command/query handlers
- **File:** Application/Agencies/Commands/ and Queries/
- **Severity:** SHOULD_FIX
- **Category:** Testing
- **Description:** No unit tests for CreateAgencyCommandHandler, AddBranchToAgencyCommandHandler, RemoveBranchFromAgencyCommandHandler, GetAgencyByIdQueryHandler, ListAgenciesForUserQueryHandler. The codebase pattern requires handler tests.
- **Fix:** Add unit tests following existing handler test patterns.

### F6: NIT — Redundant validator rule
- **File:** `Api/Validators/CreateAgencyRequestValidator.cs:16`
- **Severity:** NIT
- **Category:** Code Quality
- **Description:** `.Must(name => !string.IsNullOrWhiteSpace(name))` is redundant after `.NotEmpty()`. Remove it.
- **Fix:** Remove line 16.

### F7: NIT — Slug regex allows consecutive hyphens
- **File:** `Domain/Agencies/AgencySlug.cs:50`
- **Severity:** NIT
- **Category:** DDD
- **Description:** Current regex `^[a-z0-9][a-z0-9-]{1,48}[a-z0-9]$` allows `test--agency`. Could disallow consecutive hyphens for cleaner URLs.
- **Fix:** Update regex to disallow consecutive hyphens. Document decision.

### F8: NIT — ExistsBySlugAsync redundant normalization
- **File:** `Infrastructure/Persistence/Repositories/AgencyRepository.cs:51`
- **Severity:** NIT
- **Category:** Code Quality
- **Description:** Slug is already validated and normalized by `AgencySlug.Create()`. The `.Trim().ToLower()` in the repository is redundant.
- **Fix:** Remove redundant normalization.

---

## Section 2: Review Comment Threads (GitHub)

### Copilot Comments (12)
1. **Missing handler tests** — mapped to F5 (SHOULD_FIX)
2. **Missing integration tests for AgenciesController** — OUT_OF_SCOPE (deferred; controller follows existing patterns, integration tests planned for Task 1.5)
3. **N+1 query** — mapped to F4 (SHOULD_FIX)
4. **Slug consecutive hyphens** — mapped to F7 (NIT)
5. **LoadBranchIdsAsync code smell** — mapped to F2 (MUST_FIX)
6. **Redundant validator** — mapped to F6 (NIT)
7. **ExistsBySlugAsync normalization** — mapped to F8 (NIT)
8. **ListForUserAsync multiple queries** — mapped to F4 (SHOULD_FIX)

### Gemini Comments (7)
1. **Security: Missing org auth check in AddBranch** — mapped to F3 (MUST_FIX)
2. **N+1 query** — mapped to F4 (SHOULD_FIX)
3. **LoadBranchIdsAsync hydration** — mapped to F2 (MUST_FIX)
4. **Controller userId boilerplate** — FALSE_POSITIVE (matches existing OrgsController pattern exactly; extracting to base controller is out of scope and differs from codebase convention)
5. **Return all validation errors** — FALSE_POSITIVE (existing OrgsController returns first error only; changing pattern here creates inconsistency)
6. **Redundant validator** — mapped to F6 (NIT)
7. **GetAgencyById auth efficiency** — SUGGESTION (deferred to optimization pass; correctness takes priority)

---

## Resolution Plan

### MUST_FIX
- [x] F1: Update `InviteRequestValidator` and `UpdateRoleRequestValidator` hardcoded role arrays
- [x] F2: Add `HydrateBranches` method to Agency, refactor repository hydration
- [x] F3: Add org membership check in `AddBranchToAgencyCommandHandler`
- [x] F4: Batch-load branch IDs in `AgencyRepository` (promoted from SHOULD_FIX)

### SHOULD_FIX
- [x] F5: Add unit tests for all command/query handlers

### NIT
- [x] F6: Remove redundant validator rule
- [x] F7: Disallow consecutive hyphens in slug regex
- [x] F8: Remove redundant normalization in ExistsBySlugAsync

### FALSE_POSITIVE
- Gemini: Controller userId boilerplate — matches existing codebase pattern
- Gemini: Return all validation errors — matches existing codebase pattern

### OUT_OF_SCOPE
- Copilot: Integration tests for AgenciesController — deferred to Task 1.5
