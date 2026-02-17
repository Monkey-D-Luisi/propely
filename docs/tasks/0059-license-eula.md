# Task: 0059 - LICENSE File and EULA

## Metadata
- ID: 0059
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #211
- Epic: `docs/backlog/epic-011-licensing.md`
- Old Issues: #46, #47
- Milestone: v1.0

## Goal
Create a root LICENSE file and End User License Agreement (EULA) defining the commercial terms for using this SaaS template.

## Context
The SaaS template is intended for commercial distribution. It needs clear licensing terms that define what buyers can and cannot do: use for building products, modify the code, but not redistribute the template itself.

## Scope
### In scope
- Root `LICENSE` file with commercial license terms
- `EULA.md` document with detailed terms
- Permitted uses (build products, modify, deploy)
- Restrictions (no redistribution of template, no resale)
- Attribution requirements
- Warranty disclaimer
- Liability limitations

### Out of scope
- Legal review (template owner should consult a lawyer)
- Open source licensing
- Software patent claims

## Requirements
- R1: LICENSE file exists at repository root
- R2: EULA document covers permitted uses and restrictions
- R3: Terms are clear and understandable
- R4: Follows common SaaS template licensing patterns

## Acceptance Criteria
- AC1: `LICENSE` file exists in root
- AC2: `EULA.md` exists in root or `docs/`
- AC3: Permitted uses clearly stated
- AC4: Restrictions clearly stated

## Implementation Steps

1. **Research** SaaS template license patterns (e.g., Shipfast, Saas-UI, Makerkit)
2. **Draft LICENSE** file with commercial terms
3. **Draft EULA.md** with detailed terms
4. **Review** for clarity and completeness
5. **Add note** that users should consult legal counsel

## Files to Create

- `LICENSE`
- `EULA.md`
- `docs/walkthroughs/0059-license-eula.md`

## Definition of Done Checklist
- [x] LICENSE file exists
- [x] EULA document exists
- [x] Terms are clear
- [x] Walkthrough updated
