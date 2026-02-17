# Task: 0065 - Design System Definition (Stitch + Tailwind)

## Metadata
- ID: 0065
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-11
- GitHub Issue: #250
- Epic: `docs/backlog/epic-007-admin-dashboards.md`
- Milestone: v1.0
- Related docs:
  - Walkthrough: `docs/walkthroughs/0065-design-system.md`
  - Roadmap: `docs/roadmap-v1.md` (Phase 0)

## Goal
Establish a consistent design system for the SaaS template before implementing any new v1.0 frontend screens. Use Google Stitch MCP to explore design directions on existing screens, select definitive design tokens, and update the project's Tailwind config accordingly.

## Context
The MVP frontend was built with shadcn/ui + Tailwind defaults. Before building v1.0 UI (work items, audit log, billing), we need a cohesive design language. Google Stitch (stitch.withgoogle.com) is available as an MCP tool in VS Code and generates HTML + Tailwind mockups from AI prompts.

## Scope
### In scope
- Use Stitch MCP to generate design variations for existing screens (login, dashboard, org settings, members, feature flags)
- Decide on color palette, typography, spacing, border-radius, shadows
- Update Tailwind config (`tailwind.config.ts`) and CSS variables to match chosen design system
- Document the design system conventions
- Establish mapping between shadcn/ui components and design patterns
- Apply the design system to existing screens for consistency

### Out of scope
- Implementing new v1.0 screens (those are separate tasks: 0049, 0048, 0046)
- Backend changes
- i18n changes (unless design requires new UI text)

## Requirements
- R1: Design system is documented with color tokens, typography, spacing scale
- R2: Tailwind config and CSS variables are updated to match the design system
- R3: Existing screens are visually consistent with the new design system
- R4: Design system is usable as a reference for future UI tasks

## Acceptance Criteria
- AC1: Tailwind config reflects the chosen design tokens (colors, fonts, spacing, etc.)
- AC2: CSS variables in `globals.css` are updated
- AC3: At least 3 existing screens have been redesigned to match the new system
- AC4: Design conventions are documented (in walkthrough or a dedicated doc)
- AC5: `npm run build` and `npm test` pass
- AC6: Visual consistency across all updated screens

## Constraints (non-negotiable)
- Use shadcn/ui components — do not introduce a new component library
- Use Lucide icons (not Material Icons from Stitch output)
- Maintain i18n support in all screens
- No breaking changes to existing functionality
- Update walkthrough

## Implementation Steps

1. **Explore with Stitch MCP**: Generate design variations for existing screens
2. **Select design tokens**: Review Stitch output, decide on palette, typography, spacing
3. **Update Tailwind config**: Modify `tailwind.config.ts` with chosen tokens
4. **Update CSS variables**: Modify `apps/web/src/app/globals.css`
5. **Apply to existing screens**: Update components to use new design tokens
6. **Document conventions**: Write design system reference in walkthrough
7. **Verify**: Build, test, visual review

## Files to Create / Modify

### Modify
- `apps/web/tailwind.config.ts`
- `apps/web/src/app/globals.css`
- Various component files in `apps/web/src/components/`
- Various page files in `apps/web/src/app/[locale]/`

## Testing Plan
- Frontend build passes
- All existing tests pass
- Manual: Visual review of updated screens

## Definition of Done Checklist
- [ ] Acceptance criteria met
- [ ] Build passes
- [ ] Tests added/updated and pass
- [ ] Formatting/analyzers pass
- [ ] No secrets committed
- [ ] Walkthrough updated
