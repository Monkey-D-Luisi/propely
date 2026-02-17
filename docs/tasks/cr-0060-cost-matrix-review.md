# Code Review: CR-0060 — Cost Matrix Review

## Metadata
- PR: #260 — `docs(infra): add GCP cost matrix for three scale tiers (#0055)`
- Branch: `feat/cost-matrix` → `main`
- CI Status: claude-review FAILURE (expected), claude SKIPPED (docs-only)

## Changed Files
- `docs/infrastructure/cost-matrix.md`
- `docs/walkthroughs/0055-cost-matrix.md`
- `docs/tasks/0055-cost-matrix.md`
- `docs/backlog/epic-009-cloud-infra.md`

## Comment Sources
- Inline review comments: 6
- General reviews: 2 (Gemini summary, Copilot summary — no additional issues beyond inline)
- Issue comments: 2 (ChatGPT Codex usage limit, Gemini PR summary — not actionable)

## Comment Classification

### MUST_FIX

1. **[Copilot #2798741508] Growth-tier HCL snippet uses non-existent variable names**
   - File: `docs/infrastructure/cost-matrix.md:326`
   - Issue: Variables like `redis_memory_size_gb`, `ai_api_min_instances` don't exist in the Terraform configs. The actual pattern uses `memory_size_gb` on `module "redis"` and `min_instances` on `module "cloud_run_*"`.
   - Fix: Replace pseudo-variable snippet with actual module call overrides

### SHOULD_FIX

2. **[Gemini #2798732478] Cloud SQL Growth tier cost inconsistency**
   - File: `docs/infrastructure/cost-matrix.md:129`
   - Issue: Instance listed as ~$45 but math gives ~$50 (1 vCPU × $30 + 3.75 GB × $5). Total range ~$50-70 doesn't match summary table's $55-70.
   - Fix: Correct to ~$50 instance cost and ~$55-70 total

3. **[Gemini #2798732491] Memorystore Growth/Scale cost range unclear**
   - File: `docs/infrastructure/cost-matrix.md:168`
   - Issue: Base cost is $70 but total says $70-100 with no explanation
   - Fix: Add note explaining the range accounts for potential memory scaling within the tier

4. **[Copilot #2798741458] Labels claim inaccurate**
   - File: `docs/infrastructure/cost-matrix.md:289`
   - Issue: Not all Terraform resources include labels — Cloud SQL and VPC modules don't define them
   - Fix: Reword to "commonly include" with guidance to add labels where missing

5. **[Copilot #2798741483] Walkthrough PR reference still says "(pending)"**
   - File: `docs/walkthroughs/0055-cost-matrix.md:6`
   - Fix: Update to actual PR #260

6. **[Copilot #2798741495] Terraform config paths missing `infra/terraform/` prefix**
   - File: `docs/infrastructure/cost-matrix.md:311`
   - Fix: Add full paths

## Behavioral Parity Checks
- [x] Redirect parity: N/A (documentation only)
- [x] Locale source correctness: N/A (documentation only)
- [x] API/UI contract parity: N/A (documentation only)
- [x] Test parity: N/A (documentation only)

## Comment Resolution Plan

- [x] Fix #1: Replace Growth-tier HCL snippet with actual module overrides
- [x] Fix #2: Correct Cloud SQL Growth tier cost to ~$50 / ~$55-70
- [x] Fix #3: Add note explaining Memorystore cost ranges
- [x] Fix #4: Reword labels claim
- [x] Fix #5: Update walkthrough PR reference to #260
- [x] Fix #6: Add `infra/terraform/` prefix to config paths
- [x] Reply to all 6 inline comments
