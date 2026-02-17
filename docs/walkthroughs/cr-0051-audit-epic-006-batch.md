# Walkthrough: cr-0051-audit-epic-006-batch

## Task Reference
`docs/tasks/cr-0051-audit-epic-006-batch.md`

## PR Reference
PR #248 — fix(audit): address all audit findings for epic 006

## Changes Made

### C1: Fix `isAllowedRedirect` open redirect vulnerability
**File:** `apps/web/src/hooks/billing.ts`

**Problem:** The `isAllowedRedirect` function used `url.startsWith(prefix)` to validate redirect URLs. This allows subdomain bypass attacks where `https://checkout.stripe.com.evil.com` passes validation because it starts with `https://checkout.stripe.com`.

**Fix:** Replaced prefix-based matching with `new URL(url).hostname` exact matching against a `Set` of allowed hostnames. Added try/catch to handle malformed URLs gracefully.

```typescript
// Before (vulnerable)
function isAllowedRedirect(url: string): boolean {
  return ALLOWED_REDIRECT_PREFIXES.some((prefix) => url.startsWith(prefix));
}

// After (secure)
function isAllowedRedirect(url: string): boolean {
  try {
    const parsed = new URL(url);
    return ALLOWED_REDIRECT_HOSTNAMES.has(parsed.hostname);
  } catch {
    return false;
  }
}
```

## Commands Run
```bash
cd apps/web && npm run build  # Build passes
```

## Verification
- Frontend build succeeds with no errors
- `new URL("https://checkout.stripe.com.evil.com").hostname` → `"checkout.stripe.com.evil.com"` (rejected)
- `new URL("https://checkout.stripe.com/cs_test_abc").hostname` → `"checkout.stripe.com"` (allowed)
- Malformed URLs throw in `new URL()` and are caught, returning `false`
