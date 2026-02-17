# Task: 0070 - Third-Party Notices (Dependency Licenses)

## Metadata
- ID: 0070
- Type: Chore
- Status: DONE
- Owner: Agent
- Created: 2026-02-13
- GitHub Issue: #275
- Epic: `docs/backlog/epic-013-presale-hardening.md`
- Milestone: v1.0

## Goal
Generate a `THIRD_PARTY_NOTICES.md` listing all redistributed dependency licenses to ensure legal compliance when distributing the product commercially.

## Context
The product redistributes hundreds of NuGet and npm packages. Without a third-party notices file, buyers face legal risk. This is standard practice for commercial software products and expected by enterprise buyers.

## Scope
### In scope
- Generate `THIRD_PARTY_NOTICES.md` at repo root
- Cover NuGet packages for `services/ai-api/` and `services/orgs-api/`
- Cover npm packages for `apps/web/`
- Create regeneration scripts for Windows and Linux/macOS
- Add CI check that warns when notices are stale (compare hash of lock files)

### Out of scope
- License compatibility analysis
- Replacing any packages based on license type

## Requirements
- R1: All direct and transitive dependencies with their license types listed
- R2: Grouped by service (AI API, Orgs API, Web)
- R3: Regeneration script works cross-platform
- R4: CI check detects stale notices

## Acceptance Criteria
- AC1: `THIRD_PARTY_NOTICES.md` exists at repo root
- AC2: Lists all NuGet packages for both .NET services
- AC3: Lists all npm packages for web app
- AC4: `scripts/generate-third-party-notices.ps1` regenerates the file on Windows
- AC5: `scripts/generate-third-party-notices.sh` regenerates the file on Linux/macOS
- AC6: CI workflow warns if notices are out of date

## Implementation Steps
1. Research tools: `dotnet-project-licenses` for NuGet, `license-checker` for npm
2. Install/configure tools
3. Write PowerShell script to generate notices from both services and web
4. Write bash script equivalent
5. Run scripts to generate initial `THIRD_PARTY_NOTICES.md`
6. Add CI check comparing lock file hashes to a stored hash
7. Test CI check locally

## Files to Create
- `THIRD_PARTY_NOTICES.md`
- `scripts/generate-third-party-notices.ps1`
- `scripts/generate-third-party-notices.sh`
- `docs/walkthroughs/0070-third-party-notices.md`

## Files to Modify
- `.github/workflows/ci.yml` (add staleness check)

## Definition of Done Checklist
- [x] `THIRD_PARTY_NOTICES.md` generated with all dependencies
- [x] Regeneration scripts work on Windows and Linux/macOS
- [x] CI check warns on stale notices
- [x] Walkthrough updated
