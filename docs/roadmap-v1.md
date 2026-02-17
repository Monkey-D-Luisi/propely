# v1.0 Roadmap

## Strategic Decisions

### 1. AI API Integration

The AI API service (`services/ai-api/`) has a fully functional `IOpenAiService` (OpenAI SDK, `gpt-5-mini`) that is registered in DI but not used by any feature. To make the AI API useful instead of decorative, we add **smart input parsing** to the work items UI.

**Decision:** Integrate `IOpenAiService` into task 0049 (Work Items Frontend) as a "Smart Fill" feature.

- Users type natural language (e.g., *"Bug en el login que no valida emails con +, prioridad alta"*)
- AI extracts structured fields: Title, Description, Status
- User reviews and edits before submitting (human-in-the-loop)
- New backend endpoint: `POST /v1/work-items/parse`
- Graceful degradation if AI API is unavailable

**Scope boundaries:** Parse only existing WorkItem fields (Title, Description, Status). Priority, DueDate, Tags are not in the current domain model and are deferred to a future iteration.

See `docs/tasks/0049-work-items-frontend.md` (Scope Extension section) for full technical details.

### 2. Design System with Google Stitch

Before implementing any v1.0 frontend tasks, establish a consistent design system using Google Stitch (`stitch.withgoogle.com`) as a design exploration tool.

**What Stitch generates:**
- Pure HTML + Tailwind CSS (via CDN)
- Custom `tailwind.config` with colors, fonts, border-radius
- Google Material Icons, Inter font
- Dark mode support, no JavaScript interactivity
- No React, no JSX — static HTML mockups only

**Stitch MCP** is integrated into VS Code alongside the GitHub MCP, enabling direct design generation from the Claude Code workflow.

**Conversion pipeline (per screen):**
1. **Stitch MCP** generates HTML + Tailwind design
2. **Review and approve** design direction with user
3. **Convert** to production React components:
   - HTML elements → shadcn/ui components (`Button`, `Input`, `Select`, `Card`, `Table`, etc.)
   - Material Icons → Lucide icons (our current library)
   - Stitch Tailwind config → our project's Tailwind config tokens
   - Static markup → interactive components with state management, API calls, i18n

### 3. v1.0 Scope

All 18 pending tasks from the backlog are kept for v1.0. Task 0065 (Design System) was added during replanning, and Epic 013 (Pre-Sale Product Hardening) added 9 more tasks after a competitive audit, bringing the total to 28 tasks. All dependencies are encoded directly in the epic files so the `next task` command follows the roadmap order automatically.

### 4. Pre-Sale Product Hardening (Epic 013)

A deep-dive audit comparing the codebase against buyer expectations for commercial SaaS starter kits (ShipFast, SaaS Pegasus, Next.js SaaS Boilerplates) identified critical product-level gaps. While the engineering is solid (Epics 001-009), the product packaging needs hardening before sales begin.

**Key gaps identified:**
- `.env.example` missing critical variables (JWT, CSRF, Stripe, SendGrid) — buyers can't configure the app
- Personal email (`monkeydluisi@outlook.com`) in seed script — unprofessional
- Privacy Policy and Terms pages are 404 — footer links to non-existent pages
- No self-service account deletion — GDPR requirement and basic SaaS expectation
- No third-party dependency notices — legal risk for commercial redistribution
- Generic "SaaS Template" name — fails to differentiate
- Multi-step setup process — DX friction loses buyers in first 15 minutes
- No secret rotation or recovery documentation — enterprise buyers need operational confidence

**Decision:** Insert Phase C-0 (Pre-Sale Hardening) before Phase C (Polish & Distribution). All hardening tasks must complete before documentation and demo tasks begin, since docs depend on a complete product.

---

## Execution Phases

### Phase 0 — Design System (prerequisite for all UI tasks)

Establish a consistent design system before building any new screens.

**Steps:**
1. Use Stitch MCP to generate design variations of existing screens (login, dashboard, org settings, members, feature flags)
2. Iterate on color palette, typography, spacing, component styles
3. Compare Stitch output with current shadcn/ui + Tailwind setup
4. Decide on definitive design tokens (colors, fonts, border-radius, shadows, etc.)
5. Document the chosen design system conventions
6. Update Tailwind config and CSS variables to match
7. Establish mapping between shadcn/ui components and Stitch design patterns

**Prerequisite for:** All UI tasks in Phase B (0049, 0048, 0046)

**Tracked as:** Task 0065 in epic 007 (dependencies: 0050, 0059, 0053 — Phase A must complete first)

### Phase A — Foundation (unblock dependencies)

| Order | Task | Description | Unblocks |
|-------|------|-------------|----------|
| A1 | 0050 | GHCR container image publishing | 0054 (Cloud Run) |
| A2 | 0059 | LICENSE file and EULA | 0060, 0062 |
| A3 | 0053 | Terraform GCP foundation | 0055 (Cost matrix) |

### Phase B — Core Features (parallel tracks, design system applied)

**Track 1 — Frontend (Stitch → review → implement):**

| Order | Task | Description |
|-------|------|-------------|
| B1.1 | 0049 | Work items frontend + AI smart input parsing |
| B1.2 | 0048 | Audit log viewer UI + export |
| B1.3 | 0046 | One-time payments (add-on) |

Each task follows the Stitch pipeline: design with Stitch MCP → user review → implement with design system.

**Track 2 — Infrastructure:**

| Order | Task | Description |
|-------|------|-------------|
| B2.1 | 0054 | Cloud Run deployment pipeline (blocked by 0050) |
| B2.2 | 0055 | Cost matrix and resource planning (blocked by 0053) |

**Track 3 — CI/CD:**

| Order | Task | Description |
|-------|------|-------------|
| B3.1 | 0051 | Semantic release and versioning |
| B3.2 | 0052 | E2E smoke tests (Playwright) |

### Phase C-0 — Pre-Sale Product Hardening (NEW — before Phase C)

Closes all product-level gaps that a buyer would encounter in their first session. Must complete before documentation/demo tasks.

**Wave 1 — Quick Wins (all independent, can run in parallel):**

| Order | Task | Description | Issue |
|-------|------|-------------|-------|
| C-0.1a | 0066 | Complete .env.example + configuration guide | #271 |
| C-0.1b | 0067 | Neutralize personal data in scripts | #272 |
| C-0.2a | 0070 | Third-party notices (THIRD_PARTY_NOTICES.md) | #275 |
| C-0.2b | 0071 | Product rebranding | #276 |

**Wave 2 — UX & DX (after dependencies from Wave 1):**

| Order | Task | Description | Blocked by | Issue |
|-------|------|-------------|------------|-------|
| C-0.3a | 0068 | Privacy Policy and Terms of Service pages | 0071 | #273 |
| C-0.3b | 0072 | One-command developer onboarding | 0066 | #277 |
| C-0.4 | 0069 | Self-service account deletion (full flow) | 0066 | #274 |
| C-0.5 | 0073 | Secret rotation + production hardening guide | 0066 | #278 |

**Wave 3 — Final documentation:**

| Order | Task | Description | Blocked by | Issue |
|-------|------|-------------|------------|-------|
| C-0.6 | 0074 | Recovery runbook | 0073 | #279 |

### Phase C — Polish & Distribution

| Order | Task | Description |
|-------|------|-------------|
| C1 | 0060 | License banner in source files (blocked by 0059) |
| C2 | 0061 | Version fingerprint |
| C3 | 0062 | Licensing guide documentation (blocked by 0059) |
| C4 | 0056 | Getting started guide — Buyer Quickstart (blocked by 0049, 0066, 0072) |
| C5 | 0057 | Developer cookbook / extension recipes |
| C6 | 0058 | README — Buyer README with feature matrix + comparison (blocked by 0056, 0071) |

### Phase D — Demo (last)

| Order | Task | Description |
|-------|------|-------------|
| D1 | 0063 | Enhanced seed data for demos |
| D2 | 0064 | Demo video / walkthrough (after all other tasks) |

---

## All 28 Tasks Summary

| Epic | ID | Task | Status | Dependencies | Phase |
|------|----|------|--------|-------------|-------|
| 006 | 0046 | One-time payments (add-on) | DONE | 0042 (done), 0048 | B1.3 |
| 007 | 0065 | Design system (Stitch + Tailwind) | DONE | 0050, 0059, 0053 | 0 |
| 007 | 0049 | Work items frontend + AI parsing | DONE | 0065 | B1.1 |
| 007 | 0048 | Audit log viewer UI + export | DONE | 0049 | B1.2 |
| 008 | 0050 | GHCR container image publishing | DONE | None | A1 |
| 008 | 0051 | Semantic release and versioning | DONE | 0050 | B3.1 |
| 008 | 0052 | E2E smoke tests (Playwright) | DONE | 0051 | B3.2 |
| 009 | 0053 | Terraform GCP foundation | DONE | None | A3 |
| 009 | 0054 | Cloud Run deployment pipeline | DONE | 0050 | B2.1 |
| 009 | 0055 | Cost matrix and resource planning | DONE | 0053 | B2.2 |
| 011 | 0059 | LICENSE file and EULA | DONE | None | A2 |
| 013 | 0066 | Complete .env.example + config guide | DONE | None | C-0.1 |
| 013 | 0067 | Neutralize personal data | DONE | None | C-0.1 |
| 013 | 0070 | Third-party notices | DONE | None | C-0.2 |
| 013 | 0071 | Product rebranding | DONE | None | C-0.2 |
| 013 | 0068 | Privacy Policy + Terms pages | DONE | 0071 | C-0.3 |
| 013 | 0072 | One-command developer onboarding | DONE | 0066 | C-0.3 |
| 013 | 0069 | Self-service account deletion | DONE | 0066 | C-0.4 |
| 013 | 0073 | Secret rotation + hardening guide | DONE | 0066 | C-0.5 |
| **013** | **0074** | **Recovery runbook** | **DONE** | **0073** | **C-0.6** |
| 011 | 0060 | License banner in source files | DONE | 0059 | C1 |
| 011 | 0061 | Version fingerprint | DONE | 0059 | C2 |
| 011 | 0062 | Licensing guide documentation | DONE | 0059 | C3 |
| 010 | 0056 | Buyer Quickstart guide | DONE | 0049, 0066, 0072 | C4 |
| 010 | 0057 | Developer cookbook | DONE | 0056 | C5 |
| 010 | 0058 | Buyer README with screenshots & badges | DONE | 0056, 0071 | C6 |
| 012 | 0063 | Enhanced seed data for demos | DONE | 0058 | D1 |
| 012 | 0064 | Demo video / walkthrough | PENDING | 0063 | D2 |

## Dependency Graph

```
Phase A (Foundation) — no deps, can run in parallel:
  0050 (GHCR) ──► 0051 (Semantic Release) ──► 0052 (E2E tests)
              └──► 0054 (Cloud Run)
  0059 (LICENSE) ──► 0060 (Banner) + 0061 (Fingerprint) + 0062 (Licensing guide)
  0053 (Terraform) ──► 0055 (Cost matrix)

Phase 0 (Design System) — blocked by Phase A:
  0050 + 0059 + 0053 ──► 0065 (Design System)

Phase B (Core) — after Phase 0:
  0065 ──► 0049 (Work Items + AI) ──► 0048 (Audit Log) ──► 0046 (One-time Payments)
  0050 ──► 0054 (Cloud Run)
  0053 ──► 0055 (Cost matrix)

Phase C-0 (Pre-Sale Hardening) — after Phase B:
  Wave 1 (independent):
    0066 (.env.example) ──┬──► 0069 (Account deletion)
                          ├──► 0072 (One-command onboarding)
                          └──► 0073 (Secret rotation) ──► 0074 (Recovery runbook)
    0067 (Neutralize data)
    0070 (Third-party notices)
    0071 (Rebranding) ──► 0068 (Privacy/Terms pages)

Phase C (Polish) — after Phase C-0:
  0059 ──► 0060, 0061, 0062
  0049 + 0066 + 0072 ──► 0056 (Buyer Quickstart) ──► 0057 (Cookbook)
  0056 + 0071 ──► 0058 (Buyer README)

Phase D (Demo) — last:
  0058 ──► 0063 (Seed Data) ──► 0064 (Demo Video)
```

## Backlog Changes Made

| Source | File | Change |
|--------|------|--------|
| Task spec | `docs/tasks/0049-work-items-frontend.md` | Added "Scope Extension" section at the end (AI smart input parsing) |
| Task spec | `docs/tasks/0065-design-system.md` | New task for Phase 0 design system |
| Epic | `docs/backlog/epic-006-billing.md` | Task 0046 deps updated: added 0048 |
| Epic | `docs/backlog/epic-007-admin-dashboards.md` | Added task 0065, AI parsing success criterion, updated deps for 0048/0049 |
| Epic | `docs/backlog/epic-008-cicd-release.md` | Task 0051 deps: added 0050; Task 0052 deps: added 0051 |
| Epic | `docs/backlog/epic-010-docs-onboarding.md` | Task 0056 deps: added 0049; Task 0057/0058 deps: added 0056 |
| Epic | `docs/backlog/epic-011-licensing.md` | Task 0061 deps: added 0059 |
| Epic | `docs/backlog/epic-012-demo-marketing.md` | Task 0063 deps: added 0058; Task 0064 deps: added 0063 |
| GitHub Issue | #201 | Updated body with AI parsing scope and acceptance criteria |
| GitHub Issue | #250 | Created for task 0065 (design system) |
| Roadmap | `docs/roadmap-v1.md` | New document (this file) |
| Workflow | `.agent.md` | Added roadmap reference in section 0.1 |
| Workflow | `.agent/rules/autonomous-workflow.md` | Added roadmap reference in Step 1 |
| --- | --- | **v1.0 Roadmap Update: Pre-Sale Hardening (2026-02-13)** |
| Epic | `docs/backlog/epic-013-presale-hardening.md` | New epic: 9 tasks for pre-sale product hardening |
| Task spec | `docs/tasks/0066-env-example-config.md` | New task: Complete .env.example + configuration guide |
| Task spec | `docs/tasks/0067-neutralize-personal-data.md` | New task: Neutralize personal data |
| Task spec | `docs/tasks/0068-privacy-terms-pages.md` | New task: Privacy Policy + Terms of Service pages |
| Task spec | `docs/tasks/0069-account-deletion.md` | New task: Self-service account deletion (full flow) |
| Task spec | `docs/tasks/0070-third-party-notices.md` | New task: Third-party notices |
| Task spec | `docs/tasks/0071-product-rebranding.md` | New task: Product rebranding |
| Task spec | `docs/tasks/0072-one-command-onboarding.md` | New task: One-command developer onboarding |
| Task spec | `docs/tasks/0073-secret-rotation-guide.md` | New task: Secret rotation + production hardening guide |
| Task spec | `docs/tasks/0074-recovery-runbook.md` | New task: Recovery runbook |
| Task spec | `docs/tasks/0056-getting-started.md` | Scope enhanced: Buyer Quickstart (Stripe, OAuth, deploy) |
| Task spec | `docs/tasks/0058-readme-screenshots.md` | Scope enhanced: Buyer README (feature matrix, comparison table) |
| Epic | `docs/backlog/epic-010-docs-onboarding.md` | Task 0056 deps: added 0066, 0072; Task 0058 deps: added 0071; scopes updated |
| GitHub Issues | #271-#279 | Created for Epic 013 tasks 0066-0074 |
