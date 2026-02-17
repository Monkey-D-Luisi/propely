# Code Review: cr-0051-audit-epic-006-batch

## PR Metadata
- **PR:** #248
- **Title:** fix(audit): address all audit findings for epic 006
- **Branch:** `fix/audit-epic-006-batch` → `main`
- **CI Status:** Orgs API Build & Test ✅, Web Build & Test ✅, claude-review ❌ (automated reviewer)

## Changed Files
42 files changed (see PR for full list). Key production files:
- `services/orgs-api/src/.../Api/Controllers/AuthController.cs`
- `services/orgs-api/src/.../Api/Controllers/BillingController.cs`
- `services/orgs-api/src/.../Api/Services/CsrfValidator.cs` (new)
- `services/orgs-api/src/.../Api/Validators/UrlValidation.cs` (new)
- `services/orgs-api/src/.../Api/Validators/CheckoutRequestValidator.cs`
- `services/orgs-api/src/.../Api/Validators/CustomerPortalRequestValidator.cs`
- `services/orgs-api/src/.../Application/Billing/BillingConfiguration.cs` (moved)
- `apps/web/src/hooks/billing.ts`

## Review Threads

### Source 1: Inline Review Comments (1)
| ID | Reviewer | File | Line | Summary |
|----|----------|------|------|---------|
| C1 | gemini-code-assist[bot] | `billing.ts` | 88 | `isAllowedRedirect` using `startsWith` is vulnerable to subdomain bypass (e.g., `https://checkout.stripe.com.evil.com`) |

### Source 2: Reviews (2)
| ID | Reviewer | State | Summary |
|----|----------|-------|---------|
| R1 | copilot[bot] | COMMENTED | Summary only, "generated no comments" |
| R2 | gemini-code-assist[bot] | COMMENTED | Summary + references C1 inline comment |

### Source 3: Issue Comments (2)
| ID | Author | Summary |
|----|--------|---------|
| IC1 | chatgpt-codex-connector | Usage limit notice (not actionable) |
| IC2 | gemini-code-assist | PR summary (not actionable) |

## Comment Resolution Plan

### MUST_FIX
- [x] **C1**: Fix `isAllowedRedirect` open redirect vulnerability — replace `url.startsWith(prefix)` with `new URL(url).hostname` exact match. The reviewer is correct: `https://checkout.stripe.com.evil.com` would bypass the current check.

### NO ACTION NEEDED
- **R1**: Copilot summary, no issues raised
- **R2**: Gemini summary, references C1 (already addressed above)
- **IC1**: ChatGPT usage limit notice
- **IC2**: Gemini summary

## Behavioral Parity Checks
- [x] Redirect parity checked — `isAllowedRedirect` is the only redirect in billing hooks; no `next` parameter propagation in billing flow
- [x] Locale source correctness checked — billing hooks use route locale for `successUrl`/`cancelUrl`/`returnUrl` construction; no locale-dependent validation
- [x] API/UI contract parity checked — `CheckoutRequest(OrgId, PlanId, SuccessUrl, CancelUrl)` and `CustomerPortalRequest(OrgId, ReturnUrl)` match frontend hook payloads
- [x] Test parity checked — 18 integration tests + 27 unit tests + 8 mapper tests cover all changed backend behavior; frontend redirect validation is a pure function with no test infrastructure changes needed

## Status
DONE
