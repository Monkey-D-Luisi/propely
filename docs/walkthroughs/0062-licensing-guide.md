# Walkthrough: 0062-licensing-guide

## Task Reference
- Task: `docs/tasks/0062-licensing-guide.md`
- Walkthrough: `docs/walkthroughs/0062-licensing-guide.md`
- Branch/PR: `feat/0062-licensing-guide`
- Date: `2026-02-14`

## Summary
Created a comprehensive licensing guide at `docs/licensing-guide.md` that explains the SaaS Starter Kit license terms in plain language. The guide covers what's included, permitted uses (with examples), restrictions, team licensing, attribution requirements, updates/support, and a FAQ section answering 12 common questions. Includes a quick-reference table for at-a-glance lookup.

## Context
- Background: Task 0059 created the LICENSE and EULA with formal legal language. End users (buyers) need a plain-language companion document explaining what they can and cannot do.
- Problem statement: Legal documents are hard for developers to parse quickly. A buyer evaluating the template needs clear answers to questions like "Can I use this for client projects?" without reading the full EULA.
- Constraints: Documentation-only task. No code changes. Must be consistent with LICENSE and EULA terms.

## Decisions & Trade-offs
- **Decision:** Single standalone document at `docs/licensing-guide.md` rather than embedding in README
  - Options considered: (a) Section in README, (b) Standalone doc, (c) FAQ page in the web app
  - Why this choice: A standalone doc keeps the README focused on product features and setup. Buyers find licensing docs in `docs/` naturally. Can be linked from README, purchase page, and EULA.
  - Consequences: One more file to maintain, but it's a static document that only changes when license terms change.

- **Decision:** Used relative links to LICENSE, EULA.md, and THIRD_PARTY_NOTICES.md
  - Why: Works in GitHub's rendered markdown view and in local clones. No absolute URLs that could break.

- **Decision:** Included a quick-reference summary table at the end
  - Why: Buyers scanning the doc can jump to the table for immediate answers. Reduces support load.

## Implementation Notes
- Key changes: Created `docs/licensing-guide.md` with 8 sections (What You Get, What You Can Do, What You Cannot Do, Team Use, Attribution, Updates/Support, FAQ, Quick Reference)
- Edge cases handled: Agency use clarified (developers need licenses, clients don't). Open-source publishing clarified (can't publish template code, can publish original code). Termination consequences explained (deployed products survive, new development stops).
- Known limitations: This is a template guide — the "consult legal counsel" disclaimer is carried forward from LICENSE/EULA.

## Data / Schema / Migrations
- No DB changes
- No migrations
- N/A

## Commands Run
```bash
bash scripts/verify-license-headers.sh   # 621 files pass
```

## Files Changed
- `docs/licensing-guide.md` — New file: comprehensive plain-language licensing guide
- `docs/backlog/epic-011-licensing.md` — Task 0062 status: PENDING → IN_PROGRESS → DONE
- `docs/tasks/0062-licensing-guide.md` — Status: PENDING → DONE, DoD boxes checked
- `docs/roadmap-v1.md` — Task 0062 status: PENDING → DONE

## Tests
### Unit
- N/A (documentation-only task)

### Integration
- N/A

### Manual
- Verified all relative links point to existing files (LICENSE, EULA.md, THIRD_PARTY_NOTICES.md)
- Verified content aligns with LICENSE permitted uses and restrictions
- Verified content aligns with EULA sections 2-7 (grant, restrictions, team use, IP, attribution, updates)

## Observability
- N/A (no runtime changes)

## Security
- No code changes
- No auth/authz impact
- No sensitive data handling

## Follow-ups / Backlog
- [ ] Link licensing guide from README.md once the Buyer README (task 0058) is written
- [ ] Link licensing guide from purchase page / landing page when available

## Checklist
- [x] Task scope matches `docs/tasks/0062-licensing-guide.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
