# UI/UX Patterns Guide — Propely Web App

> **Single source of truth** for all UI implementation tasks.
> Every screen must follow these patterns to ensure visual consistency.

## 1. Design Tokens (from `globals.css`)

| Token | Value | Usage |
|-------|-------|-------|
| `primary-600` | `#5048e5` | Brand color, buttons, active states |
| `bg-surface` | `#f6f6f8` | Page backgrounds (never use `bg-slate-50`) |
| `border-slate-200` | — | All borders (cards, inputs, dividers) |
| Font | Inter | System font stack |

### Color Classes

| Purpose | Classes |
|---------|---------|
| Primary button | `bg-primary-600 hover:bg-primary-600/90 text-white shadow-sm active:scale-[0.98]` |
| Secondary button | `border border-slate-200 bg-white text-slate-700 hover:bg-slate-50` |
| Destructive action | `text-red-600 hover:bg-red-50` |
| Focus ring (buttons) | `focus:ring-2 focus:ring-primary-600 focus:ring-offset-2` |
| Focus ring (inputs) | `focus:ring-2 focus:ring-primary-600 focus:border-transparent` |
| Active nav item | `bg-primary-600/10 text-primary-600` + filled icon |
| Inactive nav item | `text-slate-600 hover:bg-slate-50` |
| Link (auth) | `text-primary-700 underline hover:text-primary-800` |

### Border Radius

| Element | Class |
|---------|-------|
| Cards / containers | `rounded-xl` |
| Inputs / buttons | `rounded-lg` |
| Badges / pills | `rounded-full` |
| Avatar | `rounded-full` |

### Icons

Material Symbols Outlined (`<span className="material-symbols-outlined">`).
Active nav items use `fontVariationSettings: "'FILL' 1"` for filled variant.

## 2. Layout Architecture

### Route Groups

| Group | Layout | Purpose |
|-------|--------|---------|
| `(marketing)` | MarketingNav + Footer | Public pages (/, pricing, legal) |
| `(auth)` | Centered card, gradient mesh | Login, register, forgot/reset password |
| `(dashboard)` | AppSidebar + main content | Core feature pages (properties, contacts, leads, appointments, work items) |
| `(app)` | AppSidebar + main content | Orgs, profile, admin, agencies |

### Sidebar Layout (mandatory for `(dashboard)` and `(app)`)

```
┌──────────────────────────────────────────────┐
│ h-dvh flex overflow-hidden                   │
│ ┌─────────┐ ┌──────────────────────────────┐ │
│ │ Sidebar │ │ main: flex-1 overflow-y-auto │ │
│ │ w-64    │ │ bg-surface                   │ │
│ │ desktop │ │                              │ │
│ │         │ │ (page content here)          │ │
│ │ hidden  │ │                              │ │
│ │ mobile  │ │                              │ │
│ └─────────┘ └──────────────────────────────┘ │
└──────────────────────────────────────────────┘
```

- Desktop (≥ md): Sidebar fixed `w-64`, always visible
- Mobile (< md): Sidebar hidden, accessible via hamburger button overlay

### Content Width

**All pages**: `max-w-6xl mx-auto` (1152px) — unified width.

Exceptions:
- Auth pages: `max-w-md` (448px, centered)
- Marketing: full-width with internal constraints

### Page Structure (standard pattern)

```tsx
<div className="flex flex-col h-full">
  {/* Page header — sticky */}
  <div className="bg-white border-b border-slate-200 px-6 py-4">
    <div className="max-w-6xl mx-auto">
      <h1 className="text-xl font-semibold text-slate-900">Page Title</h1>
      <p className="text-sm text-slate-500 mt-1">Subtitle</p>
    </div>
  </div>
  {/* Scrollable content */}
  <div className="flex-1 overflow-y-auto">
    <div className="max-w-6xl mx-auto px-6 py-6">
      {/* content */}
    </div>
  </div>
</div>
```

## 3. AI Integration Points

The AI Command Bar (`Ctrl+K`) is the core interaction. It must be **discoverable** through:

1. **Sidebar AI entry** — "AI Assistant" nav item with `auto_awesome` (sparkle) icon
2. **Floating AI button (FAB)** — bottom-right on all app pages, `auto_awesome` icon
3. **Keyboard shortcut hint** — visible in sidebar footer: `⌘K` / `Ctrl+K`
4. **Dashboard suggestions** — AI-generated suggestion feed

### AI Button (FAB) Spec

```
Position: fixed bottom-6 right-6 (z-40)
Size: h-14 w-14 (56px)
Style: bg-primary-600 text-white rounded-full shadow-lg
Hover: bg-primary-600/90 shadow-xl scale-105
Icon: auto_awesome (28px)
Pulse: subtle pulse animation on first visit
```

## 4. Component Patterns

### Empty State

```tsx
<div className="flex flex-col items-center justify-center py-16 text-center">
  <span className="material-symbols-outlined text-[48px] text-slate-300">icon_name</span>
  <h3 className="mt-4 text-lg font-medium text-slate-900">Title</h3>
  <p className="mt-1 text-sm text-slate-500 max-w-sm">Description</p>
  <Link href="/..." className="mt-4 bg-primary-600 hover:bg-primary-600/90 text-white rounded-lg px-4 py-2 text-sm font-medium shadow-sm">
    CTA Button
  </Link>
</div>
```

### Loading State

Use `Skeleton` components from `@/components/ui/skeleton/skeleton`. Every page has a `loading.tsx` file.

### Error State

Use `<ErrorMessage>` from `@/components/ui/ErrorMessage`. Display retry button when applicable.

### Status Badges

Consistent pill style: `rounded-full px-2.5 py-0.5 text-xs font-medium` with color variants per status.

### Confirmation Dialogs

Use `<DialogOverlay>` from `@/components/ui/dialog-overlay` for all destructive actions.

## 5. i18n

- All user-visible text must use `useTranslations()` with appropriate namespace
- Both `en.json` and `es.json` must be updated simultaneously
- Namespaces: `common`, `auth`, `properties`, `contacts`, `leads`, `appointments`, `dashboard`, `commandBar`, `suggestions`, `workItems`, `orgs`, etc.

## 6. Responsive Breakpoints

| Breakpoint | Width | Behavior |
|------------|-------|----------|
| Default | < 768px | Mobile: no sidebar, hamburger overlay, single column |
| `md` | ≥ 768px | Desktop: sidebar visible, multi-column layouts |

## 7. Checklist for Every UI Task

- [ ] Follows design tokens (no hardcoded colors for brand/action)
- [ ] Uses `bg-surface` for page background
- [ ] Content width = `max-w-6xl mx-auto`
- [ ] Has loading, empty, and error states
- [ ] All text uses i18n (en + es)
- [ ] Responsive: works on mobile (< 768px) and desktop
- [ ] AI button/sidebar entry visible on authenticated pages
- [ ] Visual tested in browser via Playwright before completion
