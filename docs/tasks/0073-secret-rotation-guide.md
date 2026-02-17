# Task: 0073 - Secret Rotation and Production Hardening Guide

## Metadata
- ID: 0073
- Type: Documentation
- Status: DONE
- Owner: Agent
- Created: 2026-02-13
- GitHub Issue: #278
- Epic: `docs/backlog/epic-013-presale-hardening.md`
- Milestone: v1.0

## Goal
Write a production hardening guide that gives a buyer confidence to deploy to production, covering secret rotation, security configuration, and pre-deploy checklists.

## Context
The product includes Terraform for GCP deployment and Secret Manager references, but there is no documentation on how to rotate secrets without downtime, how to pin Secret Manager versions, or what security configurations must be set before going live. A buyer evaluating the product for production use needs this documentation.

### Secrets That Need Rotation Procedures
- JWT signing secret (used in `Jwt__Secret`)
- CSRF secret (used in `Csrf__Secret`)
- Stripe webhook secret
- OAuth client secrets (Google, GitHub)
- Database credentials (Cloud SQL)
- SendGrid API key
- OpenAI API key

## Scope
### In scope
- Write `docs/production-hardening.md`
- JWT secret rotation with zero-downtime dual-key window approach
- CSRF, Stripe, OAuth, SendGrid, OpenAI secret rotation procedures
- Database credential rotation via GCP Secret Manager
- Terraform changes: pin Secret Manager versions (not `latest`)
- Pre-deploy security checklist (CORS, CSP, rate limiting, HTTPS, etc.)
- Production environment variable checklist

### Out of scope
- Implementing automated rotation (documentation only)
- SOC2 / compliance documentation
- Penetration testing

## Requirements
- R1: Each secret type has a step-by-step rotation procedure
- R2: JWT rotation explains dual-key window for zero downtime
- R3: Terraform examples show pinned Secret Manager versions
- R4: Pre-deploy checklist is actionable (not vague)

## Acceptance Criteria
- AC1: `docs/production-hardening.md` exists
- AC2: All 7 secret types have documented rotation procedures
- AC3: JWT dual-key rotation is explained with code examples
- AC4: Terraform Secret Manager pinning is demonstrated
- AC5: Pre-deploy checklist covers CORS, CSP, rate limiting, HTTPS, cookies
- AC6: Production env var checklist distinguishes required vs optional

## Implementation Steps
1. Review current Terraform Secret Manager usage
2. Review JWT validation code for dual-key support feasibility
3. Write JWT secret rotation section with dual-key approach
4. Write Stripe webhook secret rotation section
5. Write OAuth client secret rotation section
6. Write database credential rotation section
7. Write SendGrid and OpenAI API key rotation sections
8. Write pre-deploy security checklist
9. Write production environment variable checklist
10. Update Terraform examples with pinned versions
11. Review and cross-reference with actual code

## Files to Create
- `docs/production-hardening.md`
- `docs/walkthroughs/0073-secret-rotation-guide.md`

## Files to Modify
- Terraform files (pin Secret Manager versions if needed)

## Definition of Done Checklist
- [x] `docs/production-hardening.md` created
- [x] All secret types covered
- [x] JWT dual-key rotation documented
- [x] Terraform pinning demonstrated
- [x] Pre-deploy checklist complete
- [x] Walkthrough updated
