# Walkthrough: cr-0010-pr19-permission-model-review

## Task Reference
- Task: `docs/tasks/cr-0010-pr19-permission-model-review.md`
- PR: #19 (`feat/permission-domain-model`)
- Date: `2026-02-22`

## Summary
Code review of PR #19 (Permission Domain Model). Addressed namespace mismatch in `EffectivePermissionDto`, semantic parameter naming in `PermissionOverrideDeniedV1`, and several documentation inaccuracies in the task and walkthrough files.

## Changes Made

### 1. MUST_FIX: EffectivePermissionDto namespace mismatch
- **File:** `EffectivePermissionDto.cs` -- Changed namespace from `Propely.OrgsApi.Application.Permissions.Interfaces` to `Propely.OrgsApi.Application.Permissions.DTOs` to match folder location and codebase convention.
- **File:** `IPermissionEvaluator.cs` -- Added `using Propely.OrgsApi.Application.Permissions.DTOs;` since `EffectivePermissionDto` is no longer in the same namespace.
- **File:** `PermissionEvaluator.cs` -- Added `using Propely.OrgsApi.Application.Permissions.DTOs;` for the same reason.

### 2. SHOULD_FIX: PermissionOverrideDeniedV1 parameter naming
- **File:** `PermissionOverrideDeniedV1.cs` -- Renamed constructor parameter `grantedAtUtc` to `deniedAtUtc` to match the `DeniedAtUtc` property name in the Data record. This eliminates a semantic mismatch where a "denied" event used "granted" terminology for its timestamp.

### 3. SHOULD_FIX: Task doc R2 soft-delete reference
- **File:** `docs/tasks/0008-permission-domain-model.md` -- Updated R2 from "follows the existing Entity pattern with factory method, domain events, and soft-delete" to "follows the existing Entity pattern with factory method and domain events (uses Revoke() instead of soft-delete)".

### 4. SHOULD_FIX: Task doc AC4 inaccuracy
- **File:** `docs/tasks/0008-permission-domain-model.md` -- Updated AC4 from "returns all permissions except owner-reserved" to "returns all permissions (same defaults as Owner; difference is Admin can be restricted via deny overrides)".

### 5. SHOULD_FIX: Walkthrough decision note
- **File:** `docs/walkthroughs/0008-permission-domain-model.md` -- Updated the EffectivePermissionDto decision note. Changed from incorrect claim about following "the existing pattern where result types are co-located with their queries" to accurately reflecting that the namespace was corrected during code review to match the DTOs folder convention.

## Commands Run
```bash
dotnet build services/orgs-api/Propely.OrgsApi.sln
dotnet test services/orgs-api/Propely.OrgsApi.sln
```

## Validation Results
- Build: (pending)
- Tests: (pending)

## Checklist
- [x] Task file created (`docs/tasks/cr-0010-pr19-permission-model-review.md`)
- [x] Walkthrough file created (`docs/walkthroughs/cr-0010-pr19-permission-model-review.md`)
- [x] All MUST_FIX items addressed
- [x] All SHOULD_FIX items addressed
- [x] FALSE_POSITIVE items documented with rationale
- [ ] Build passes
- [ ] Tests pass
