# Task: 0024 - User Profile Page

## Metadata
- ID: 0024
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-02-04
- Related docs:
  - Epic: `docs/backlog/epic-001-professional-saas-refinement.md`
  - Walkthrough: `docs/walkthroughs/0024-user-profile.md`

## Goal
Add a user profile page where users can edit their display name and change their password. Implement backend endpoints and frontend form with react-hook-form and i18n.

## Context
Users currently can only register and login. There's no way to update profile information or change password. A SaaS template needs basic profile management.

## Scope
### In scope
- Backend: `PATCH /auth/me` endpoint (update name)
- Backend: `PUT /auth/password` endpoint (change password - requires current password)
- Frontend: `/profile` page with two forms: profile info and change password
- Frontend: Link to profile in AppHeader (user email dropdown or icon)
- Frontend: react-hook-form + Zod validation for both forms
- Frontend: i18n for all strings

### Out of scope
- Profile photo/avatar
- Email change (requires verification flow)
- Account deletion
- Two-factor authentication

## Requirements
- R1: User can update their display name
- R2: Password change requires current password verification
- R3: New password has minimum length validation (8 chars)
- R4: Profile page is only accessible when authenticated
- R5: All strings from i18n

## Acceptance Criteria
- AC1: `PATCH /auth/me` updates user name
- AC2: `PUT /auth/password` changes password after verifying current password
- AC3: Wrong current password returns 401
- AC4: Profile page shows current user info pre-filled
- AC5: Both forms show success/error toasts
- AC6: Profile link visible in AppHeader
- AC7: Page works in both EN and ES
- AC8: `dotnet build` + `dotnet test` pass
- AC9: `npm run build` + `npm run lint` pass

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Implementation Steps
1. Create `UpdateProfileCommand` + handler
2. Create `ChangePasswordCommand` + handler (verify old, hash new)
3. Add PATCH /auth/me and PUT /auth/password endpoints
4. Create frontend profile page with two form sections
5. Add profile link to AppHeader
6. Add i18n strings
7. Test

## Files to Create / Modify
### Backend
- `Application/Users/Commands/UpdateProfile/` (create)
- `Application/Users/Commands/ChangePassword/` (create)
- `Api/Controllers/AuthController.cs` (modify)

### Frontend
- `apps/web/src/app/[locale]/profile/page.tsx` (create)
- `apps/web/src/components/profile/ProfileForm.tsx` (create)
- `apps/web/src/components/profile/ChangePasswordForm.tsx` (create)
- `apps/web/src/components/layout/AppHeader.tsx` (modify - add profile link)
- `apps/web/messages/en.json` (modify)
- `apps/web/messages/es.json` (modify)

## Testing Plan
- Unit tests: Handler tests for both commands
- Integration tests: API endpoint tests
- Frontend tests: Form component tests
- Manual verification: Update name, change password

## Security & Privacy
- Current password required for password change (prevent CSRF-based password change)
- New password hashed with BCrypt
- CSRF protection on both endpoints

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
