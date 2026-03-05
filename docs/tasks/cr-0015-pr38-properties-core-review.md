# CR-0015: PR #38 — Properties Core Code Review

## PR Metadata

- **PR**: #38 — feat(properties): Epic P2 — Properties core domain, API, media, SDK client
- **Branch**: `feat/p2-properties-core` → `main`
- **CI Status (pre-review)**: License Headers Check FAILED; others queued/passing

## Changed Files

93 files across:
- `apps/web/` — JSON Forms renderers, schemas, tests
- `services/properties-api/src/` — Domain, Application, Infrastructure, Api, Client
- `services/properties-api/tests/` — Unit tests

---

## Section 1: Agent Review Findings

### MUST_FIX

| # | File | Category | Description | Status |
|---|------|----------|-------------|--------|
| 1 | 9 frontend `.tsx`/`.ts` files | CI/Code Quality | Missing license headers — CI failure | FIXED |
| 2 | `EnergyRating.cs` / `property-schema.json` / `EnergyRatingRenderer.tsx` | API-UI Contract Parity | Frontend schema has `InProgress` enum value not present in backend `EnergyRating` enum | FIXED — Added `InProgress = 8` to backend enum |
| 3 | `UploadMediaCommandHandler.cs:68` | Robustness/Security | `request.FileStream.Position = 0` assumes seekable stream — non-seekable streams will throw | FIXED — Copy to MemoryStream first |
| 4 | `UploadMediaCommandHandler.cs:39-44` | Security | No quantity limit for `FloorPlan` uploads — resource exhaustion risk | FIXED — Added `MaxFloorPlansPerProperty = 10` + `CountByPropertyIdAndMediaTypeAsync` |
| 5 | `ReorderMediaCommandHandler.cs:28-35` | Data Integrity | Partial reorder allowed, duplicate DisplayOrder possible | FIXED — Validate all items present + unique display orders |

### SHOULD_FIX

| # | File | Category | Description | Status |
|---|------|----------|-------------|--------|
| 6 | `PropertyMediaController.cs:94-103` | Code Quality | `ReorderRequest`/`ReorderItemRequest` DTOs defined inside controller file | FIXED — Moved to `Api/Dtos/PropertyMediaRequests.cs` |

---

## Section 2: Review Comment Threads

### Source: Gemini Code Assist (1 review, 5 inline comments)

| # | Comment | Classification | Action |
|---|---------|---------------|--------|
| G1 | EnergyRating frontend/backend mismatch | MUST_FIX | FIXED — merged with Agent finding #2 |
| G2 | Stream not seekable in UploadMediaCommandHandler | MUST_FIX | FIXED — merged with Agent finding #3 |
| G3 | ReorderMedia allows partial/duplicate orders | MUST_FIX | FIXED — merged with Agent finding #5 |
| G4 | No floor plan upload limit (security) | MUST_FIX | FIXED — merged with Agent finding #4 |
| G5 | Move ReorderRequest DTOs to Dtos directory | SHOULD_FIX | FIXED — merged with Agent finding #6 |

### Source: Issue Comments

| # | Author | Body | Classification |
|---|--------|------|---------------|
| I1 | chatgpt-codex-connector | Usage limit reached — no review provided | N/A |
| I2 | gemini-code-assist | Summary of changes | N/A — informational only |

---

## Resolution Plan

- [x] MUST_FIX #1: Add license headers (9 files)
- [x] MUST_FIX #2: Add `InProgress = 8` to `EnergyRating` enum
- [x] MUST_FIX #3: Copy FileStream to MemoryStream in UploadMediaCommandHandler
- [x] MUST_FIX #4: Add floor plan limit (MaxFloorPlansPerProperty = 10, new repo method)
- [x] MUST_FIX #5: Validate reorder completeness and display order uniqueness
- [x] SHOULD_FIX #6: Move ReorderRequest DTOs to PropertyMediaRequests.cs
- [ ] Build and test
- [ ] Commit, push, verify CI
- [ ] Merge PR
