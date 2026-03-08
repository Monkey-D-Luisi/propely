# Code Review: cr-0025 — PR #73 Shared TenantDelegatingHandler

## PR Metadata
- **PR**: [#73](https://github.com/Monkey-D-Luisi/propely/pull/73) — `refactor(shared): extract TenantDelegatingHandler into Propely.Shared.Http (#0053)`
- **Branch**: `feat/0053-shared-tenant-delegating-handler` → `main`
- **CI Status**: Pending review fixes
- **Changed files**: 26 files (2 created, 19 modified, 5 deleted)

## Section 1: Agent Review Findings

### A.2.1 Architecture & Clean Architecture
- OK — Shared project has no framework dependencies beyond ASP.NET Core FrameworkReference (needed for `IHttpContextAccessor`). Dependencies flow inward correctly.

### A.2.7 Inter-Service Communication
- **MUST_FIX** [F1]: Docker build broken — `ai-api/Dockerfile` does not COPY the new `services/shared/Propely.Shared.Http/` project, so `dotnet restore` will fail because the 3 external client `.csproj` files now reference it via `ProjectReference`.
- **MUST_FIX** [F2]: Docker dev broken — `ai-api/Dockerfile.dev` client stubs don't include the `Shared.Http` ProjectReference, and `docker-compose.yml` doesn't mount the shared project directory.

### A.2.8 Code Quality
- **SHOULD_FIX** [F3]: Walkthrough says "7 test files" updated but only 6 were actually modified (verified from diff). Inaccurate documentation.
- **SHOULD_FIX** [F4]: Epic AC5 "Docker build succeeds with the new project reference" marked `[x]` but Docker wasn't updated — should be reverted to `[ ]` until Docker fix is applied.

### A.3 Behavioral Parity
- No issues — pure refactoring, no behavior change.

## Section 2: Review Comment Threads

### Gemini Code Assist [inline, medium priority]
- **File**: `docs/walkthroughs/0053-shared-tenant-delegating-handler.md` line 24
- **Claim**: Summary says "7 test files" but only 6 were updated
- **Verification**: CORRECT — confirmed via grep, only 6 test files contain `using Propely.Shared.Http;`
- **Classification**: SHOULD_FIX → merged with [F3]

### Copilot [inline, comment 1]
- **File**: `services/properties-api/src/Propely.PropertiesApi.Client/Propely.PropertiesApi.Client.csproj`
- **Claim**: `ai-api/Dockerfile`, `docker-compose.yml`, and `ai-api/Dockerfile.dev` need updates for the shared project
- **Verification**: CORRECT — Dockerfiles don't copy/mount the shared project
- **Classification**: MUST_FIX → merged with [F1] and [F2]

### Copilot [inline, comment 2]
- **File**: `docs/backlog/epic-P8-ai-provider-abstraction.md` line 31
- **Claim**: AC5 "Docker build succeeds" shouldn't be marked done
- **Verification**: CORRECT — Docker hasn't been updated
- **Classification**: SHOULD_FIX → merged with [F4]

## Resolution Plan

### MUST_FIX
- [x] [F1] Update `services/ai-api/Dockerfile` to COPY `services/shared/Propely.Shared.Http/` in both restore and build stages
- [x] [F2a] Create `services/ai-api/client-stubs/Propely.Shared.Http.csproj` stub
- [x] [F2b] Update all 3 existing client stubs to include `ProjectReference` to Shared.Http
- [x] [F2c] Update `services/ai-api/Dockerfile.dev` to COPY the Shared.Http stub
- [x] [F2d] Update `docker-compose.yml` ai-api volumes to mount `./services/shared/Propely.Shared.Http`

### SHOULD_FIX
- [x] [F3] Fix walkthrough: change "7 test files" to "6 test files"
- [x] [F4] Revert epic AC5 to unchecked, then re-check after Docker fix is verified
