# Task: cr-0005 — PR #144 Code Review

## PR Metadata
- **PR:** #144 — feat(i18n): add ES translations and language switcher (#0006)
- **Branch:** feat/0006-es-translations-switcher → main
- **CI Status:** All checks passed (Web Build SUCCESS, Detect Changes SUCCESS, Claude Review passed)

## Changed Files
| File | +/- |
|------|-----|
| apps/web/messages/en.json | +4 / -1 |
| apps/web/messages/es.json | +124 / -121 |
| apps/web/src/app/[locale]/layout.tsx | +19 / -8 |
| apps/web/src/components/layout/AppHeader.tsx | +2 / -0 |
| apps/web/src/components/layout/LanguageSwitcher.tsx | +35 / -0 |
| docs/backlog/epic-001-professional-saas-refinement.md | +4 / -4 |
| docs/tasks/0006-es-translations-switcher.md | +6 / -6 |
| docs/walkthroughs/0006-es-translations-switcher.md | +89 / -0 |

## Comment Counts
- Inline review comments: 10
- General reviews: 4
- Issue comments: 2

## Review Threads

### 1. Codex — Preserve query params on locale switch (LanguageSwitcher.tsx:21)
Switching locales uses `usePathname()` only, dropping query parameters. Breaks flows relying on query strings (login `next`/`reauth`, invite `token`).

### 2. Gemini — params is not a Promise (layout.tsx:17)
Claims `params` in Next.js layouts should be a plain object, not a Promise.

### 3. Gemini — Ellipsis inconsistency x3 (es.json:11,42,65)
`"Cargando..."` / `"Creando cuenta..."` / `"Creando..."` should use `…` (single character) for consistency.

### 4. Gemini — Globe emoji identical (LanguageSwitcher.tsx:31)
Globe emoji 🌐 is the same for both locales. Suggests using flag emojis.

### 5. Copilot — Globe emoji identical (LanguageSwitcher.tsx:31)
Same as #4. Suggests 🇺🇸/🇪🇸.

### 6. Copilot — aria-label mismatch (LanguageSwitcher.tsx:32)
aria-label says "Español"/"English" but button displays "ES"/"EN".

### 7. Copilot — appDescription mentions Fastify (es.json:4)
Description says "Fastify" but the project uses .NET APIs.

### 8. Copilot — x-default should be constant (layout.tsx:27)
x-default hreflang should point to a constant fallback, not dynamic per locale.

## Comment Resolution Plan

### MUST_FIX
- [x] #1 — Preserve query params when switching locale (use `useSearchParams()` + pathname + Suspense boundary)
- [x] #8 — Fix x-default hreflang to constant `/en` (default locale via `routing.defaultLocale`)

### SHOULD_FIX
- [x] #3 — Replace `...` with `…` in both en.json and es.json for consistency (3 occurrences each)
- [x] #4/#5 — Simplify globe emoji (remove dead conditional, keep single 🌐)
- [x] #7 — Fix appDescription in both en.json and es.json (Fastify → .NET)

### QUESTION (respond with rationale)
- [x] #6 — aria-label is intentionally more descriptive than visual text (standard a11y practice). Replied.

### OUT_OF_SCOPE (reject with explanation)
- [x] #2 — Gemini is wrong: in Next.js 15+/16, params IS a Promise in layouts. Current code is correct. Replied.
