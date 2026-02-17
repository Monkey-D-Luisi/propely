# Walkthrough: 0059 - LICENSE File and EULA

## Task Reference
- Task: `docs/tasks/0059-license-eula.md`
- Walkthrough: `docs/walkthroughs/0059-license-eula.md`
- Branch/PR: `feat/0059-license-eula` / TBD
- Date: 2026-02-12

## Summary
Created a root LICENSE file and EULA.md for the SaaS template, establishing commercial licensing terms. The license follows common SaaS template patterns (similar to Shipfast, Makerkit, etc.): buyers can use, modify, and deploy the code for their own products but cannot redistribute the template itself.

## Context
- Background: The SaaS template is intended for commercial distribution and needs clear licensing terms.
- Problem statement: No LICENSE or EULA existed, leaving usage rights undefined.
- Constraints: Not legal advice — not reviewed by legal counsel. Includes disclaimer to consult a lawyer. No open source license.

## Decisions & Trade-offs
- **License model: Proprietary commercial license**
  - Options considered: MIT, Apache 2.0, BSL, proprietary commercial
  - Why this choice: The template is a commercial product. Open source licenses would allow free redistribution. A proprietary license protects the author's commercial interest while granting broad usage rights to buyers.
  - Consequences / risks: Users must purchase a license. The license is a template itself — the owner should have a lawyer review it.

- **Two-file structure: LICENSE + EULA.md**
  - LICENSE is a short summary (what GitHub displays)
  - EULA.md has the full detailed terms

## Implementation Notes
- Key changes: Two new files at repository root
- Edge cases handled: Multi-project use, team access, derivative works
- Known limitations: Not reviewed by legal counsel — template owner should do this

## Data / Schema / Migrations
- N/A — documentation-only task

## Commands Run
```bash
git checkout main && git pull origin main
git checkout -b feat/0059-license-eula
# Create LICENSE and EULA.md
```

## Files Changed
- `LICENSE` — Short-form commercial license (GitHub-visible summary)
- `EULA.md` — Full End User License Agreement with detailed terms

## Tests
### Unit
- N/A — no code changes

### Integration
- N/A — no code changes

### Manual
- Verified LICENSE renders on GitHub repo page
- Verified EULA.md is readable and well-structured

## Observability
- N/A

## Security
- No code changes — licensing terms only

## Follow-ups / Backlog
- [ ] Task 0060: License banner in source files (depends on 0059)
- [ ] Task 0061: Version fingerprint (depends on 0059)
- [ ] Task 0062: Licensing guide documentation (depends on 0059)
- [ ] Owner should consult a lawyer for final legal review

## Checklist
- [x] Task scope matches `docs/tasks/0059-license-eula.md`
- [x] Validation: docs verified/rendering validated (no code tests applicable)
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
