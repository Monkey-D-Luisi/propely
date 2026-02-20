# Code Review: cr-0004-pr14-scaffold-review

## Metadata
- PR: #14 (`feat/0003-scaffold-new-services`)
- Target branch: main
- CI status: claude-review FAILURE (disabled), all other checks PASS/SKIPPED
- Review date: 2026-02-20

## Changed Files
255 files added across 4 new service directories + 3 doc files updated.

---

## Section 1: Agent Review Findings

### Finding A1: Stale Dockerfile comment in publishing-api
- **Severity:** MUST_FIX
- **Category:** Code Quality / Rename completeness
- **Files:** `services/publishing-api/Dockerfile` (line 2)
- **Description:** Comment says "Properties API" instead of "Publishing API". Copy-paste artifact.
- **Fix:** Update comment to "Publishing API".

### Finding A2: Stale Dockerfile comment in contacts-api
- **Severity:** MUST_FIX
- **Category:** Code Quality / Rename completeness
- **Files:** `services/contacts-api/Dockerfile` (line 2)
- **Description:** Comment says "Properties API" instead of "Contacts API". Copy-paste artifact.
- **Fix:** Update comment to "Contacts API".

### Finding A3: Unused NuGet package in Application .csproj
- **Severity:** SHOULD_FIX
- **Category:** Code Quality / Dead dependency
- **Files:** All 4 services `src/Propely.<Service>.Application/Propely.<Service>.Application.csproj`
- **Description:** `Microsoft.Extensions.Configuration.Abstractions` v10.0.2 is included but not used by any Application-layer code. Not present in ai-api reference.
- **Fix:** Remove the unused package reference from all 4 Application .csproj files.

### Finding A4: Missing SSL cert validation comment in RedisCacheService
- **Severity:** NIT
- **Category:** Code Quality / Documentation
- **Files:** All 4 services `src/Propely.<Service>.Infrastructure/Caching/RedisCacheService.cs`
- **Description:** ai-api has `// Trust Google Memorystore CA cert (private VPC traffic, not publicly trusted)` before the SSL cert bypass. New services omit this explanatory comment.
- **Fix:** Add the comment for clarity.

### Finding A5: XML doc comments stripped compared to ai-api
- **Severity:** NIT
- **Category:** Code Quality / Consistency
- **Files:** Multiple files across all 4 services
- **Description:** XML doc comments present in ai-api are systematically absent in scaffolded services. Non-functional but inconsistent.
- **Fix:** Skip for now; can be added incrementally when domain logic is implemented.

---

## Section 2: Review Comment Threads

### Thread R1: Codex Bot - Outbox dispatcher without migrations (P1)
- **Source:** chatgpt-codex-connector[bot], inline on `DependencyInjection.cs:46`
- **Claim:** OutboxDispatcherService is registered unconditionally but no migrations exist to create outbox_messages/processed_events tables, causing startup failure loop.
- **Classification:** OUT_OF_SCOPE
- **Rationale:** This is identical to the ai-api pattern. The scaffold intentionally has no migrations. The DatabaseMigrationConfiguration runs `MigrateAsync()` on startup, but with no migrations, it's a no-op. The first domain entity task will create an initial migration that includes outbox/processed_events tables (they're already configured via EF Core entity configurations). During development, the outbox dispatcher will log warnings but not crash (it catches exceptions). Additionally, `appsettings.Testing.json` already has `OutboxDispatcher:Enabled=false`.

### Thread R2: Codex Bot - FOR UPDATE SKIP LOCKED without transaction (P1)
- **Source:** chatgpt-codex-connector[bot], inline on `OutboxRepository.cs:34`
- **Claim:** Row locks from `FOR UPDATE SKIP LOCKED` are released immediately because no surrounding transaction wraps the select+publish+mark-processed cycle.
- **Classification:** OUT_OF_SCOPE
- **Rationale:** This is copied from ai-api's working implementation. The suggestion is technically valid but affects all 6 services, not just the 4 new ones. This should be addressed as a cross-service improvement in a dedicated task. The current at-least-once semantics are acceptable (duplicate events are idempotent-safe via processed_events table).

### Thread R3: Copilot - Dockerfile comment stale (publishing-api)
- **Source:** Copilot, inline on `services/publishing-api/Dockerfile:3`
- **Claim:** Comment says "Properties API" but should say "Publishing API".
- **Classification:** MUST_FIX (same as A1)

### Thread R4: Copilot - Dockerfile comment stale (contacts-api)
- **Source:** Copilot, inline on `services/contacts-api/Dockerfile:3`
- **Claim:** Comment says "Properties API" but should say "Contacts API".
- **Classification:** MUST_FIX (same as A2)

### Thread R5-R8: Copilot - FOR UPDATE SKIP LOCKED (all 4 services)
- **Source:** Copilot, inline on `OutboxRepository.cs:36` (x4 services)
- **Claim:** Same as R2. Locks not held across transaction.
- **Classification:** OUT_OF_SCOPE (same as R2)

---

## Resolution Plan

### MUST_FIX
- [x] A1/R3: Fix publishing-api Dockerfile comment
- [x] A2/R4: Fix contacts-api Dockerfile comment

### SHOULD_FIX
- [x] A3: Remove unused `Microsoft.Extensions.Configuration.Abstractions` from all 4 Application .csproj files

### NIT (skip)
- [ ] A4: SSL comment in RedisCacheService (cosmetic, skip for now)
- [ ] A5: XML doc comments (too many files, skip for scaffold PR)

### OUT_OF_SCOPE (documented, no action)
- [ ] R1: Outbox dispatcher without migrations (by design for empty scaffold)
- [ ] R2/R5-R8: FOR UPDATE SKIP LOCKED transaction pattern (cross-service improvement, defer)
