# ft-0006: Fix Service Audits

## Status: IN PROGRESS

## Overview

Address all findings from the comprehensive service audits across all three services:
ai-api (18 items), orgs-api (20 items), web (22 items). Total: 60 action items.

## Requirements

### ai-api (18 items)
- P0: 1 item (RBAC authorization policies)
- P1: 4 items (JWT validation, auth logging, composite index, read repo tests)
- P2: 4 items (projector tests, userId helper, OpenAiService exception, LIKE escaping)
- P3: 9 items (unit tests, caching, status parsing, health check docs, CSP, OTel)

### orgs-api (20 items)
- P0: 2 items (admin auth on AuditLogs, admin auth on FeatureFlags)
- P1: 6 items (CSRF HMAC, ILike escape, Stripe webhook validation, export row limit, admin bypass tests, OAuth tests)
- P2: 7 items (JWT min length, PII in logs, N+1 fix, outbox index, CSRF tests, domain entity tests, health check)
- P3: 5 items (GetUserId extraction, BillingController dependency, AcceptInviteRequest, dead-letter, secure cookies)

### web (22 items)
- P0: 1 item (open redirect fix)
- P1: 5 items (CSP headers, billing tests, AcceptInvite tests, i18n fix, useMemo fix, focus ring fix)
- P2: 6 items (console.error, notification tests, roles tests, dialog tests, type assertions, form-demo removal)
- P3: 8 items (font, dynamic imports, MembersManager decompose, i18n StatusBadge, dialog overlay, auth-events tests, next/image, error.body validation)

## Source Reports
- `docs/audits/service-ai-api-audit.md`
- `docs/audits/service-orgs-api-audit.md`
- `docs/audits/service-web-audit.md`

## DOD
- [ ] All P0 items addressed
- [ ] All P1 items addressed
- [ ] All P2 items addressed
- [ ] All P3 items addressed
- [ ] ai-api builds and tests pass
- [ ] orgs-api builds and tests pass
- [ ] web builds and tests pass
- [ ] Audit reports updated (all items marked Done)
- [ ] Walkthrough completed
- [ ] PR created
