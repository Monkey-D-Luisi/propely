# FT-0001: Stitch Design Audit & Migration

## Goal

Audit and migrate all Propely screen designs to the correct Stitch project (`11823747923864658899`), ensuring every application page has a detailed, design-system-consistent Stitch screen with its HTML reference downloaded to the repository.

## Context

After the P2 epic project change, the original Stitch designs were in project `16786124142182555397` which does not belong to this development. All designs needed to be regenerated in the correct project with consistent design system tokens and maximum detail.

## Scope

### In Scope
- Audit all 36+ Next.js pages to identify every screen needing a Stitch design
- Generate all screens in the correct Stitch project (`11823747923864658899`)
- Create a Design System Reference Sheet as the first screen
- Download all HTML references to `.stitch-html/`
- Update `CLAUDE.md` to reference the correct single Stitch project
- Update agent memory to prevent future use of the wrong project

### Out of Scope
- Visual pixel-perfect verification of implementations vs designs (follow-up task)
- Code changes to match new designs
- Deletion of old Stitch project (not owned by this development)

## Acceptance Criteria

- [x] All 43 screens generated in Stitch project `11823747923864658899`
- [x] All 43 HTML files downloaded to `.stitch-html/`
- [x] No files from the old/wrong project remain
- [x] `CLAUDE.md` updated with single correct project reference
- [x] Design System Reference Sheet exists as baseline
- [x] Every Next.js page has a corresponding Stitch design

## Screen Inventory (43 screens)

### Design System (1)
- Design System Reference Sheet

### Auth (8)
- Login, Register, Forgot Password, Reset Link Sent, Set New Password
- Email Verification: Loading, Success, Error

### Marketing (4)
- Landing Page, Pricing, Privacy Policy, Terms of Service

### Organization Management (6)
- Organizations List, Organization Settings, Team Members
- Organization Billing, User Permissions, Member Permissions Matrix

### Accept Invite (3)
- Loading, Success, Error

### Properties (4)
- Properties Grid View, Property Detail, New Property Wizard, Edit Property

### Contacts & Leads (4)
- Contacts Management, Contact CRM Detail, Leads Kanban Pipeline, Appointments Calendar

### Work Items (4)
- Work Items Management, Work Item Detail, New Work Item, Edit Work Item

### Agencies (3)
- New Agency Form, Agency Detail Dashboard, Agency Settings

### Profile & Admin (4)
- User Profile, Admin Feature Flags, Admin Audit Logs, Admin System Health

### Error Pages (2)
- 404 Error, Unexpected Error

## Files Modified

- `CLAUDE.md` — Updated Stitch project references
- `.stitch-html/*.html` — 43 HTML design reference files
