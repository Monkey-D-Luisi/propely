# Propely — Web Frontend

Next.js 16 + React 19 + Tailwind CSS v4 frontend for the Propely monorepo.

## Tech Stack

- **Next.js 16** (App Router, Server Components)
- **React 19** with TypeScript
- **Tailwind CSS v4** with semantic design tokens
- **next-intl** for i18n (English + Spanish)
- **react-hook-form + Zod** for form validation
- **JSON Forms** (`@jsonforms/react`) for schema-driven property forms
- **FullCalendar** for appointment calendar views
- **Vitest + Testing Library** for unit/component tests
- **Playwright** for E2E tests

## Development

From the monorepo root:

```bash
# Full stack (recommended)
./scripts/dev-up.sh          # Linux/macOS
.\scripts\dev-up.ps1         # Windows

# Or run web only (requires infra + APIs running separately)
./scripts/run-web.sh         # Linux/macOS — http://localhost:3000
.\scripts\run-web.ps1        # Windows
```

## Build & Test

```bash
cd apps/web
npm run build     # Production build
npm test          # Unit + component tests (Vitest)
npm run lint      # ESLint
```

## Project Structure

```
src/
  app/[locale]/       Pages (App Router, i18n)
  components/         React components by domain
    auth/             Login, Register, OAuth
    orgs/             Organization management
    billing/          Subscription & payments
    ui/               Shared UI components
    layout/           Header, sidebar, navigation
  hooks/              Custom React hooks (API calls, state)
  lib/                Utilities, schemas, API client
  messages/           i18n translation files (en.json, es.json)
e2e/                  Playwright E2E tests
```

## Design System

Design tokens are defined in `src/app/globals.css` via Tailwind v4 `@theme`. Use semantic `primary-*` classes for brand/action colors. See `CLAUDE.md` for the full design system conventions.

## Environment Variables

The web app reads `NEXT_PUBLIC_*` variables from the root `.env`:

| Variable | Default | Purpose |
|----------|---------|---------|
| `NEXT_PUBLIC_ORGS_API_URL` | `http://localhost:5020` | Orgs API base URL |
| `NEXT_PUBLIC_AI_API_URL` | `http://localhost:5010` | AI API base URL |
| `NEXT_PUBLIC_PROPERTIES_API_URL` | `http://localhost:5030` | Properties API base URL |
| `NEXT_PUBLIC_CONTACTS_API_URL` | `http://localhost:5050` | Contacts API base URL |
| `NEXT_PUBLIC_APPOINTMENTS_API_URL` | `http://localhost:5060` | Appointments API base URL |
| `NEXT_PUBLIC_PUBLISHING_API_URL` | `http://localhost:5040` | Publishing API base URL |

## Port

| Service | Port |
|---------|------|
| Web frontend | 3000 |
