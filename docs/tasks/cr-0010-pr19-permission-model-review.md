# Task: cr-0010-pr19-permission-model-review

## Metadata
- ID: cr-0010
- Type: Code Review
- PR: #19 (`feat/permission-domain-model`)
- Target branch: `main`
- CI status: Orgs API, Properties API, Publishing API, License Headers, Third-Party Notices, Detect Changes = SUCCESS; AI API, Contacts API, Appointments API = IN_PROGRESS/QUEUED; Web = SKIPPED

## Changed Files
- `docs/backlog/epic-P1-agency-permissions.md` (status updates)
- `docs/tasks/0008-permission-domain-model.md` (new task doc)
- `docs/walkthroughs/0008-permission-domain-model.md` (new walkthrough)
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/DTOs/EffectivePermissionDto.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Interfaces/IPermissionEvaluator.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Interfaces/IPermissionOverrideRepository.cs`
- `services/orgs-api/src/Propely.OrgsApi.Application/Permissions/Services/PermissionEvaluator.cs`
- `services/orgs-api/src/Propely.OrgsApi.Domain/Permissions/DefaultPermissionMatrix.cs`
- `services/orgs-api/src/Propely.OrgsApi.Domain/Permissions/Events/PermissionOverrideDeniedV1.cs`
- `services/orgs-api/src/Propely.OrgsApi.Domain/Permissions/Events/PermissionOverrideGrantedV1.cs`
- `services/orgs-api/src/Propely.OrgsApi.Domain/Permissions/Events/PermissionOverrideRevokedV1.cs`
- `services/orgs-api/src/Propely.OrgsApi.Domain/Permissions/Permission.cs`
- `services/orgs-api/src/Propely.OrgsApi.Domain/Permissions/PermissionOverride.cs`
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Application/Permissions/PermissionEvaluatorTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Domain/Permissions/DefaultPermissionMatrixTests.cs`
- `services/orgs-api/tests/Propely.OrgsApi.UnitTests/Domain/Permissions/PermissionOverrideTests.cs`

---

## Section 1: Agent Review Findings

### A.2.1 Architecture & Clean Architecture
- All checks pass. Domain has zero framework dependencies. Dependencies flow inward. No circular references. Correct namespace convention `Propely.OrgsApi.<Layer>`.

### A.2.2 Domain Design (DDD)
- `PermissionOverride` enforces invariants via factory. Value objects (Permission enum) are immutable. Domain events named in past tense with V1 suffix. Events include all required metadata fields.

### A.2.3 API & Security
- N/A (no API or security changes in this PR).

### A.2.4 Data & Persistence
- N/A (pure domain model, no persistence changes).

### A.2.5 Testing
- 77+ unit tests covering all role x permission x override combinations. AAA pattern followed. Happy path and error paths covered. No logic in tests.

### A.2.6 Frontend
- N/A (no frontend changes).

### A.2.7 Inter-Service Communication
- N/A (no inter-service changes).

### A.2.8 Code Quality
- **Finding 1 (MUST_FIX):** `EffectivePermissionDto.cs` is in `Permissions/DTOs/` folder but declares namespace `Propely.OrgsApi.Application.Permissions.Interfaces`. Violates the folder-matches-namespace convention used consistently across the codebase.
- **Finding 2 (SHOULD_FIX):** `PermissionOverrideDeniedV1` constructor parameter is named `grantedAtUtc` but the Data record property is `DeniedAtUtc`. Semantic mismatch creates confusion.
- **Finding 3 (SHOULD_FIX):** Task doc R2 references "soft-delete" but the implementation intentionally does not implement `ISoftDeletable`. Doc should match the actual design decision.
- **Finding 4 (SHOULD_FIX):** Task doc AC4 says "returns all permissions except owner-reserved" but the implementation (correctly per design) gives Admin ALL permissions. Doc should be updated.
- **Finding 5 (SHOULD_FIX):** Walkthrough decision note for EffectivePermissionDto placement claims it follows "the existing pattern where result types are co-located with their queries" -- this is inaccurate; DTOs in the codebase use separate DTOs folders with matching namespaces.

### A.3 Behavioral Parity Checks
- N/A (pure domain model, no API/UI changes).

---

## Section 2: Review Comment Threads

### Source 1: Inline Review Comments (14 total)

| # | Reviewer | File | Severity | Summary | Classification |
|---|----------|------|----------|---------|----------------|
| 1 | Copilot | `PermissionOverrideDeniedV1.cs:30` | SHOULD_FIX | Constructor param `grantedAtUtc` should be `deniedAtUtc` to match Data record | SHOULD_FIX |
| 2 | Copilot | `EffectivePermissionDto.cs:6` | MUST_FIX | Namespace mismatch: `Interfaces` should be `DTOs` | MUST_FIX |
| 3 | Copilot | `docs/tasks/0008-permission-domain-model.md:48` | SHOULD_FIX | AC4 doc/code discrepancy | SHOULD_FIX |
| 4 | Copilot | `docs/walkthroughs/0008-permission-domain-model.md:21` | SHOULD_FIX | Decision note inaccurate | SHOULD_FIX |
| 5 | Gemini | `epic-P1-agency-permissions.md:69` | NIT | Status update confirmation | FALSE_POSITIVE |
| 6 | Gemini | `epic-P1-agency-permissions.md:221` | NIT | Status update confirmation | FALSE_POSITIVE |
| 7 | Gemini | `epic-P1-agency-permissions.md:940` | NIT | Status update confirmation | FALSE_POSITIVE |
| 8 | Gemini | `docs/tasks/0008-permission-domain-model.md:6` | NIT | Task status confirmation | FALSE_POSITIVE |
| 9 | Gemini | `docs/tasks/0008-permission-domain-model.md:38` | SHOULD_FIX | R2 mentions soft-delete but implementation uses Revoke() | SHOULD_FIX |
| 10 | Gemini | `docs/tasks/0008-permission-domain-model.md:48` | SHOULD_FIX | AC4 doc/code discrepancy (duplicate of #3) | SHOULD_FIX |
| 11 | Gemini | `DefaultPermissionMatrix.cs:17` | SHOULD_FIX | Suggests Admin should exclude owner-reserved permissions | FALSE_POSITIVE |
| 12 | Gemini | `PermissionOverride.cs:12` | SHOULD_FIX | Missing ISoftDeletable | FALSE_POSITIVE |
| 13 | Gemini | `PermissionEvaluatorTests.cs:122` | NIT | Owner deny override test observation | FALSE_POSITIVE |
| 14 | Gemini | `DefaultPermissionMatrixTests.cs:31` | SHOULD_FIX | Test vs AC4 contradiction | SHOULD_FIX |

### Source 2: General Reviews (2 total)
- Copilot: Summary review, no additional action items beyond inline comments.
- Gemini: Summary review highlighting R2/AC4 doc-code discrepancies (covered above).

### Source 3: Issue Comments (2 total)
- ChatGPT Codex: Usage limit message (not a review).
- Gemini: PR summary (informational, no action).

---

## Resolution Plan

### MUST_FIX
- [x] Fix `EffectivePermissionDto.cs` namespace from `Permissions.Interfaces` to `Permissions.DTOs` (Agent #1, Copilot #2)
- [x] Add `using Propely.OrgsApi.Application.Permissions.DTOs;` to `IPermissionEvaluator.cs`
- [x] Add `using Propely.OrgsApi.Application.Permissions.DTOs;` to `PermissionEvaluator.cs`

### SHOULD_FIX
- [x] Rename `grantedAtUtc` → `deniedAtUtc` in `PermissionOverrideDeniedV1` constructor (Agent #2, Copilot #1)
- [x] Update task doc R2 to remove "soft-delete" mention (Agent #3, Gemini #9)
- [x] Update task doc AC4 to say "returns all permissions" instead of "except owner-reserved" (Agent #4, Copilot #3, Gemini #10, #14)
- [x] Update walkthrough decision note about EffectivePermissionDto (Agent #5, Copilot #4)

### FALSE_POSITIVE (no action, rationale)
- Gemini #5-8: Informational status confirmations, no action needed.
- Gemini #11: Suggests Admin should have fewer defaults than Owner. This is incorrect per the design decision: Admin and Owner share the same defaults, but Admin CAN be restricted via deny overrides while Owner cannot. The code is correct; only the documentation was inaccurate.
- Gemini #12: Suggests PermissionOverride should implement ISoftDeletable. This is an intentional design decision documented in the walkthrough: overrides use `Revoke()` with domain events instead. The documentation (R2) was updated to match.
- Gemini #13: Informational observation about the Owner deny override test. The test correctly verifies the business rule.
- ChatGPT Codex: Usage limit message, not a review comment.
- Gemini issue comment: Informational PR summary.

---

## Definition of Done
- [x] All agent findings addressed
- [x] All reviewer comments addressed
- [ ] Build passes
- [ ] Tests pass
- [ ] CI green
- [ ] PR merged
