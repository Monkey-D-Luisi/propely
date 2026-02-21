# Walkthrough: cr-0009 — PR #18 Agency Entity Review (Pass 2)

## Task Reference
- Task: `docs/tasks/cr-0009-pr18-agency-review-pass2.md`
- PR: #18 (`feat/0007-agency-entity-hierarchy`)
- Previous review: cr-0008

## Changes Made

### F1: Fix slug regex mismatch in CreateAgencyCommandValidator
- `Application/Agencies/Commands/CreateAgency/CreateAgencyCommandValidator.cs` — Updated regex from `^[a-z0-9][a-z0-9-]{1,48}[a-z0-9]$` to `^[a-z0-9](?:[a-z0-9]|-(?!-))*[a-z0-9]$` to match Domain and API layers

### F2+F3: Add index and FK on organizations.agency_id
- `Infrastructure/Persistence/Configurations/OrganizationConfiguration.cs` — Added `HasOne<Agency>().WithMany().HasForeignKey(o => o.AgencyId).OnDelete(DeleteBehavior.SetNull)` and `HasIndex(o => o.AgencyId)`
- `Infrastructure/Migrations/20260221211426_AddAgencyIdIndexAndForeignKey.cs` — New migration adding index and FK constraint

### F4: Fix DateTime.UtcNow drift
- `Domain/Agencies/Agency.cs` — Captured `var now = DateTime.UtcNow` once in `AddBranch()` and `RemoveBranch()` methods, reused for both entity timestamp and domain event

### F7: Fix duplicate OccurredAtUtc in branch event data
- `Domain/Agencies/Events/BranchAddedToAgencyV1.cs` — Renamed `BranchAddedToAgencyV1Data.OccurredAtUtc` to `AddedAtUtc`
- `Domain/Agencies/Events/BranchRemovedFromAgencyV1.cs` — Renamed `BranchRemovedFromAgencyV1Data.OccurredAtUtc` to `RemovedAtUtc`

### F8: Add Agent role rejection test
- `tests/.../Commands/AddBranchToAgencyCommandHandlerTests.cs` — Added test for `MembershipRole.Agent` rejection in org membership check

## Commands Run
```bash
dotnet ef migrations add AddAgencyIdIndexAndForeignKey   # Generated migration for index + FK
dotnet build services/orgs-api/Propely.OrgsApi.sln       # 0 errors, 25 warnings (pre-existing)
dotnet test services/orgs-api/tests/.../UnitTests/       # 520 passed (was 519)
```

## Validation Results
- Build: PASS (0 errors)
- Unit tests: 520 passed, 0 failed (1 new Agent rejection test added)
