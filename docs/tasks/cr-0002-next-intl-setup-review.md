# Code Review: PR #140 - next-intl infrastructure setup

## Metadata
- ID: cr-0002
- Type: Code Review
- Status: DONE
- PR: [#140](https://github.com/Monkey-D-Luisi/saas-template/pull/140)
- Branch: `feat/0003-next-intl-setup` → `main`
- CI Status: Pending
- Created: 2026-02-05

## PR Context
- **Title:** feat(web): setup next-intl infrastructure with locale routing (#0003)
- **Files Changed:** 24 files
- **Related Task:** [0003-next-intl-setup.md](./0003-next-intl-setup.md)

## Review Comment Summary

| Source | Count |
|--------|-------|
| Inline review comments | 3 |
| General reviews | 2 |
| Issue comments | 2 |

## Review Threads (Unresolved)

### Thread 1: `apps/web/src/app/[locale]/layout.tsx` (line 46)
- **Reviewer:** gemini-code-assist
- **Marked as:** CRITICAL
- **Claim:** 
  1. `params` prop should be plain object, not Promise
  2. `NextIntlClientProvider` requires `locale` and `messages` props

### Thread 2: `apps/web/src/i18n/request.ts` (line 6)
- **Reviewer:** gemini-code-assist  
- **Marked as:** CRITICAL
- **Claim:** Should destructure `locale` directly instead of `requestLocale`

### Thread 3: `apps/web/next.config.ts` (line 6)
- **Reviewer:** copilot-pull-request-reviewer
- **Marked as:** SUGGESTION
- **Claim:** Pass explicit path to `createNextIntlPlugin('./src/i18n/request.ts')`

---

## Comment Analysis (Against Official Docs)

### Thread 1 Analysis: `layout.tsx`
**Documentation Reference:** https://next-intl.dev/docs/getting-started/app-router/with-i18n-routing

**Verification:**
1. **`params` handling:** Official docs show `params: Promise<{locale: string}>` with `await params` pattern. Current code matches exactly.
2. **`NextIntlClientProvider` props:** Docs at https://next-intl.dev/docs/usage/configuration#nextintlclientprovider state: "These props are inherited if you're rendering NextIntlClientProvider from a Server Component: locale, messages, now, timeZone, formats". Props are **not required**.

**Verdict:** ❌ **INCORRECT** - Both claims are wrong. Code follows official next-intl v4 docs exactly.

### Thread 2 Analysis: `request.ts`
**Documentation Reference:** https://next-intl.dev/docs/getting-started/app-router/with-i18n-routing

**Verification:** Official docs show:
```typescript
export default getRequestConfig(async ({requestLocale}) => {
  const requested = await requestLocale;
  // ...
});
```

Current code matches this pattern exactly.

**Verdict:** ❌ **INCORRECT** - Code follows official docs. Reviewer appears to be using outdated information.

### Thread 3 Analysis: `next.config.ts`
**Verification:** Default path is `./src/i18n/request.ts`. Being explicit adds clarity and future-proofs against default changes.

**Verdict:** ✅ **VALID SUGGESTION** - Low priority, good for maintainability.

---

## Comment Resolution Plan

### SUGGESTION (Implement if trivial)
- [ ] **Thread 3:** Add explicit path to `createNextIntlPlugin('./src/i18n/request.ts')`

### QUESTION (Respond with explanation)
- [ ] **Thread 1:** Reply explaining code follows official next-intl v4 docs for Next.js 15+/16
- [ ] **Thread 2:** Reply explaining code matches official docs exactly, with reference

---

## Actions Taken
- [x] Fetched all PR context (files, reviews, comments)
- [x] Verified comment claims against official next-intl v4 documentation
- [x] Implement Thread 3 suggestion
- [x] Reply to Thread 1 with explanation
- [x] Reply to Thread 2 with explanation
- [x] Commit and push changes
- [x] Update walkthrough
