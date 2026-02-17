# Task: 0067 - Neutralize Personal Data in Scripts and Examples

## Metadata
- ID: 0067
- Type: Chore
- Status: DONE
- Owner: Agent
- Created: 2026-02-13
- GitHub Issue: #272
- Epic: `docs/backlog/epic-013-presale-hardening.md`
- Milestone: v1.0

## Goal
Remove all author-specific personal data from the distributed codebase to ensure a professional, neutral product.

## Context
The pre-sale audit found personal data in scripts — specifically `monkeydluisi@outlook.com` in `scripts/seed-dev.sh`. A commercial product must not contain any author-specific references. All placeholders must use RFC-compliant domains (`.test`, `.example`).

### Known Instances
- `scripts/seed-dev.sh` line 12: `INVITE_EMAIL="${INVITE_EMAIL:-monkeydluisi@outlook.com}"`

## Scope
### In scope
- Audit ALL scripts, seed files, Docker Compose files, Terraform examples, and documentation for personal data
- Replace with neutral placeholders (e.g., `invitee@example.com`, `demo-user@saastemplate.test`)
- Verify no personal GitHub usernames, URLs, or emails remain
- Check git blame is not a concern (this is about distributed source, not git history)

### Out of scope
- Git history rewriting
- Changing functional behavior of scripts

## Requirements
- R1: Zero personal emails, names, or URLs in distributed files
- R2: All placeholder emails use `.test` or `.example` TLDs per RFC 2606
- R3: Scripts must remain functional after replacement

## Acceptance Criteria
- AC1: `grep -r "monkeydluisi"` across the repo returns zero results (excluding .git)
- AC2: All seed/example emails use `.test` or `.example` domains
- AC3: Scripts still work correctly after changes
- AC4: Full audit documented in walkthrough

## Implementation Steps
1. Run full-repo grep for known personal identifiers (email, username, name)
2. Run broader grep for non-standard email domains in scripts
3. Replace all instances with neutral placeholders
4. Test that `seed-dev.sh` still runs correctly
5. Document all changes in walkthrough

## Files to Modify
- `scripts/seed-dev.sh`
- Any other files found during audit

## Files to Create
- `docs/walkthroughs/0067-neutralize-personal-data.md`

## Definition of Done Checklist
- [x] Zero personal data in codebase
- [x] All placeholders use RFC-compliant domains
- [x] Scripts tested and functional
- [x] Walkthrough updated
