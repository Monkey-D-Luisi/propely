# Task: cr-0009 — PR #18 Agency Entity Review (Pass 2)

## PR Metadata
- PR: #18 (`feat/0007-agency-entity-hierarchy`)
- Base: main
- CI: Orgs API PASSING, AI API PASSING. Properties/Publishing/Contacts/Appointments API FAILING (unrelated NuGet vuln).
- Previous review: cr-0008 (all 8 findings resolved)
- Reviews: All 19 reviewer comments (Copilot: 12, Gemini: 7) replied to in cr-0008

## Changed Files
73 files changed — see PR description for full list.

---

## Section 1: Agent Review Findings (Pass 2)

### F1: SHOULD_FIX — Slug regex mismatch in CreateAgencyCommandValidator
- **File:** `Application/Agencies/Commands/CreateAgency/CreateAgencyCommandValidator.cs:24`
- **Severity:** SHOULD_FIX
- **Category:** Input Validation / Consistency
- **Description:** Application-layer validator uses old regex `^[a-z0-9][a-z0-9-]{1,48}[a-z0-9]$` which allows consecutive hyphens. The Domain (`AgencySlug.cs:50`) and API (`CreateAgencyRequestValidator.cs:25`) validators both use the updated regex `^[a-z0-9](?:[a-z0-9]|-(?!-))*[a-z0-9]$`. A slug like `test--slug` would pass Application validation but fail at Domain creation with a generic exception instead of a proper validation error.
- **Fix:** Update the regex to match Domain/API layers. Ideally reference a shared constant, but at minimum sync the pattern.

### F2: SHOULD_FIX — Missing index on organizations.agency_id
- **File:** `Infrastructure/Persistence/Configurations/OrganizationConfiguration.cs`, Migration
- **Severity:** SHOULD_FIX
- **Category:** Data & Persistence / Performance
- **Description:** Repository queries filter on `organizations.agency_id` (both `GetBranchIdsForAgencyAsync` and `ListForUserAsync`). Without an index, these are full table scans.
- **Fix:** Add `HasIndex(o => o.AgencyId)` to OrganizationConfiguration and generate migration.

### F3: SHOULD_FIX — No FK constraint from organizations.agency_id to agencies.id
- **File:** `Infrastructure/Persistence/Configurations/OrganizationConfiguration.cs`, Migration
- **Severity:** SHOULD_FIX
- **Category:** Data & Persistence / Integrity
- **Description:** Both entities are in the same bounded context and database. Without FK, orphaned agency_id references can exist. A FK with `OnDelete(SetNull)` ensures referential integrity.
- **Fix:** Add `HasOne<Agency>().WithMany().HasForeignKey(o => o.AgencyId).OnDelete(DeleteBehavior.SetNull)` and generate migration.

### F4: NIT — Multiple DateTime.UtcNow calls in AddBranch/RemoveBranch
- **File:** `Domain/Agencies/Agency.cs:81,83,92,94`
- **Severity:** NIT
- **Category:** Code Quality
- **Description:** `AddBranch()` and `RemoveBranch()` call `DateTime.UtcNow` twice each (once for UpdatedAtUtc, once for domain event). Can produce microsecond drift. `Create()` correctly captures `var now = DateTime.UtcNow` once.
- **Fix:** Capture `var now = DateTime.UtcNow` once and reuse.

### F5: NIT — AddBranch/RemoveBranch return 200 OK with empty body
- **File:** `Api/Controllers/AgenciesController.cs:91,102`
- **Severity:** NIT
- **Category:** API Design
- **Classification:** FALSE_POSITIVE — Existing controllers (BillingController, OrgsController) use `return Ok()` for void operations. This matches the codebase pattern.

### F6: NIT — CreateAgency uses StatusCode(201) without Location header
- **File:** `Api/Controllers/AgenciesController.cs:52`
- **Severity:** NIT
- **Category:** API Design
- **Classification:** FALSE_POSITIVE — Existing controllers (OrgsController, AuthController) use `StatusCode(201, ...)` for creation. This matches the codebase pattern.

### F7: NIT — Duplicate OccurredAtUtc in branch event data
- **File:** `Domain/Agencies/Events/BranchAddedToAgencyV1.cs:33`, `BranchRemovedFromAgencyV1.cs:33`
- **Severity:** NIT
- **Category:** Domain Design
- **Description:** `BranchAddedToAgencyV1Data.OccurredAtUtc` duplicates the envelope-level `IDomainEvent.OccurredAtUtc`. Compare with `AgencyCreatedV1Data.CreatedAtUtc` which is semantically distinct.
- **Fix:** Rename to `AddedAtUtc` and `RemovedAtUtc` respectively.

### F8: NIT — Missing test for MembershipRole.Agent rejection
- **File:** `tests/.../Commands/AddBranchToAgencyCommandHandlerTests.cs`
- **Severity:** NIT
- **Category:** Testing
- **Description:** Tests exist for null membership, Viewer rejection, and Admin/Owner success. No explicit test for Agent role rejection — the central behavioral change of this PR.
- **Fix:** Add theory test case for MembershipRole.Agent.

---

## Section 2: Review Comment Threads (GitHub)

All 19 reviewer comments (12 Copilot, 7 Gemini) from the original review were addressed and replied to in cr-0008. No new reviewer comments were posted since the cr-0008 fixes were pushed.

---

## Resolution Plan

### SHOULD_FIX
- [x] F1: Update slug regex in `CreateAgencyCommandValidator` to match Domain/API layers
- [x] F2+F3: Add index and FK constraint on `organizations.agency_id` via new migration

### NIT (implement)
- [x] F4: Capture `DateTime.UtcNow` once in `AddBranch`/`RemoveBranch`
- [x] F7: Rename `OccurredAtUtc` to `AddedAtUtc`/`RemovedAtUtc` in branch event data records
- [x] F8: Add `MembershipRole.Agent` rejection test case

### FALSE_POSITIVE
- F5: `return Ok()` matches existing codebase pattern for void operations
- F6: `StatusCode(201, ...)` matches existing codebase pattern for creation
