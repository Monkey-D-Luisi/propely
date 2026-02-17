# Walkthrough: 0070-third-party-notices

## Task Reference
- Task: `docs/tasks/0070-third-party-notices.md`
- Walkthrough: `docs/walkthroughs/0070-third-party-notices.md`
- Branch/PR: `feat/0070-third-party-notices` / TBD
- Date: `2026-02-14`

## Summary
Generated `THIRD_PARTY_NOTICES.md` at the repository root listing all 181 redistributed dependencies (125 NuGet + 56 npm) with their licenses. Created cross-platform regeneration scripts (PowerShell + Bash) and added a CI staleness check that warns when dependency lock files change without regenerating notices.

## Context
- Background: The product redistributes hundreds of NuGet and npm packages. Without a third-party notices file, buyers face legal risk when redistributing the product commercially.
- Problem statement: No third-party dependency notices existed in the repository.
- Constraints: Must work cross-platform (Windows PowerShell + Linux/macOS Bash). Must be regeneratable after dependency updates.

## Decisions & Trade-offs
- **Decision:** Used NuGet.org registration API (with catalog entry follow) for license lookup
  - Options considered: (a) `dotnet-project-licenses` tool, (b) Direct NuGet.org API, (c) Parse .nuspec from local cache
  - Why this choice: The NuGet.org API provides authoritative license data without installing additional tools. The catalog entry must be followed with a second HTTP request since the registration endpoint returns it as a URL rather than an embedded object.
  - Consequences / risks: Requires internet access during script execution. 7 packages had non-standard API registrations, handled with a known-license fallback map.

- **Decision:** Used `npx license-checker` for npm packages
  - Options considered: (a) `license-checker`, (b) `license-report`, (c) Manual package.json parsing
  - Why this choice: `license-checker` is the most widely used npm license tool, supports CSV output, and handles production-only filtering. Using `npx` avoids requiring a global install.
  - Consequences / risks: Deprecation warnings from `license-checker` dependencies (inflight, glob v7), but output is correct.

- **Decision:** CI check warns (not fails) on stale notices
  - Options considered: (a) Fail the build, (b) Warning annotation
  - Why this choice: Third-party notices staleness is not a correctness issue and shouldn't block PRs. A warning is sufficient to remind developers to regenerate.
  - Consequences / risks: Notices could remain stale if warnings are ignored, but the hash comparison makes it obvious.

- **Decision:** Added known-license fallback map for 7 packages
  - Options considered: (a) Leave as "Unknown", (b) Hardcode known licenses
  - Why this choice: Packages like MediatR (Apache-2.0), Stripe.net (Apache-2.0), BCrypt.Net-Next (MIT) have well-known licenses. Leaving them as "Unknown" would look unprofessional.
  - Consequences / risks: If a package changes its license, the fallback map needs updating. This is low-risk for established packages.

## Implementation Notes
- Key changes:
  - `THIRD_PARTY_NOTICES.md`: 321 lines, 3 sections (AI API NuGet, Orgs API NuGet, Web npm), zero "Unknown" licenses
  - `scripts/generate-third-party-notices.ps1`: PowerShell script with NuGet API + npx license-checker
  - `scripts/generate-third-party-notices.sh`: Bash equivalent with jq + curl for NuGet, npx for npm
  - `.third-party-notices-hash`: SHA256 hash of all dependency-defining files for CI staleness check
  - `.github/workflows/ci.yml`: New `third-party-notices` job that compares dependency hashes
- Only `src/` .csproj files are scanned (not test projects) since those are the redistributed packages
- License resolution: NuGet registration API → catalog entry URL → license data, with URL normalization for common patterns

## Commands Run
```bash
# Script development and testing
powershell -ExecutionPolicy Bypass -File scripts/generate-third-party-notices.ps1

# Quality checks
dotnet build services/ai-api/SaasTemplate.AiApi.sln      # OK
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln   # OK
cd apps/web && npm run build                               # OK
```

## Files Changed
- `THIRD_PARTY_NOTICES.md` — new, auto-generated dependency license listing
- `.third-party-notices-hash` — new, SHA256 hash for CI staleness detection
- `scripts/generate-third-party-notices.ps1` — new, PowerShell regeneration script
- `scripts/generate-third-party-notices.sh` — new, Bash regeneration script
- `.github/workflows/ci.yml` — added `third-party-notices` staleness check job
- `docs/backlog/epic-013-presale-hardening.md` — task 0070 status updated to DONE

## Tests
### Unit
- All existing builds pass (no code changes, only scripts and documentation)

### Verification
- PowerShell script successfully generates THIRD_PARTY_NOTICES.md with 125 NuGet + 56 npm packages
- Zero "Unknown" license entries in generated file
- Hash file generated for CI comparison

## Security
- Validation: No secrets involved — only public package metadata
- AuthN/AuthZ impact: None
- Sensitive data handling: None

## Follow-ups
- [ ] Verify bash script works on Linux/macOS (CI will exercise this on first PR)
- [ ] Update known-license fallback map if packages change licenses
