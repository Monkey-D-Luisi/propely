# Walkthrough: 0024-user-profile

## Task Reference
- Task: `docs/tasks/0024-user-profile.md`
- Walkthrough: `docs/walkthroughs/0024-user-profile.md`
- Branch/PR: `feat/0024-user-profile`
- Date: `2026-02-07`

## Summary
Added a user profile page where authenticated users can edit their display name and change their password. Implemented two backend endpoints (`PATCH /auth/me` and `PUT /auth/password`), two frontend form components, and full i18n support in EN and ES.

## Context
- Background: Users could register and log in but had no way to update their profile information or change their password after registration. Basic profile management is a fundamental SaaS feature.
- Problem statement: No profile editing or password change functionality existed.
- Constraints: Clean Architecture layers respected. CSRF required for password changes. Current password must be verified before allowing change. All strings internationalized.

## Decisions & Trade-offs
- **Domain methods on User entity:**
  - Added `UpdateProfile()` and `ChangePassword()` directly on the User entity following the existing pattern (private setters, domain methods).
  - Name is trimmed and set to null if empty/whitespace, matching the `User.Create()` behavior.

- **CSRF only on password change, not profile update:**
  - `PUT /auth/password` requires CSRF (security-sensitive mutation).
  - `PATCH /auth/me` does not require CSRF (follows the pattern: only mutations with security implications require CSRF, and profile name is low-risk).

- **No new repository methods needed:**
  - EF Core change tracking handles the update automatically. The handler calls `GetByIdAsync`, modifies the entity, and calls `SaveChangesAsync`. No `UpdateAsync` method was added to `IUserRepository`.

- **AppHeader shows name instead of email:**
  - Changed the header to display `user.name || user.email` and made it a link to `/profile`. This provides better UX when users have a display name set.

## Implementation Notes
- Key changes:
  - User entity gained `UpdateProfile()` and `ChangePassword()` domain methods
  - Two new CQRS items: `UpdateProfileCommand` (returns result) and `ChangePasswordCommand` (void)
  - Two new API endpoints on AuthController: `PATCH /auth/me` and `PUT /auth/password`
  - Two new DTOs and validators for request validation
  - Frontend profile page at `/[locale]/profile` with two forms
  - ProfileForm: editable name field, disabled email field (read-only)
  - ChangePasswordForm: current password + new password, CSRF-protected
  - Profile link in AppHeader replaces static email text
  - New `profile` i18n namespace with all strings in EN and ES
  - New schemas: `createUpdateProfileFormSchema` and `createChangePasswordFormSchema`
  - New hooks: `useUpdateProfile` and `useChangePassword`
- Edge cases handled:
  - Null/whitespace name clears the name (sets to null)
  - Wrong current password returns 401
  - New password too short (< 8 chars) returns 400
  - Unauthenticated access returns 401
  - Missing CSRF on password change returns 403
  - Form resets after successful password change
  - Loading state shows skeleton placeholders
  - Error state shows error message
- Known limitations: Email cannot be changed (requires verification flow, out of scope).

## Commands Run
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln
cd apps/web && npm run build
cd apps/web && npx vitest run
```

## Files Changed

### Backend - Domain
- `services/orgs-api/src/SaasTemplate.OrgsApi.Domain/Users/User.cs` — Added `UpdateProfile()` and `ChangePassword()` methods

### Backend - Application
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Commands/UpdateProfile/UpdateProfileCommand.cs` — New: command and result records
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Commands/UpdateProfile/UpdateProfileCommandHandler.cs` — New: handler
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Commands/ChangePassword/ChangePasswordCommand.cs` — New: command record
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Commands/ChangePassword/ChangePasswordCommandHandler.cs` — New: handler with password verification

### Backend - API
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/AuthController.cs` — Added PATCH /auth/me and PUT /auth/password endpoints
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Dtos/UpdateProfileRequest.cs` — New: request DTO
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Dtos/ChangePasswordRequest.cs` — New: request DTO
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Validators/UpdateProfileRequestValidator.cs` — New: FluentValidation validator
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Validators/ChangePasswordRequestValidator.cs` — New: FluentValidation validator

### Frontend
- `apps/web/src/app/[locale]/profile/page.tsx` — New: profile page with both forms
- `apps/web/src/components/profile/ProfileForm.tsx` — New: profile info form (name edit)
- `apps/web/src/components/profile/ChangePasswordForm.tsx` — New: change password form
- `apps/web/src/components/layout/AppHeader.tsx` — Changed email to name/email link to profile
- `apps/web/src/hooks/orgs.ts` — Added `useUpdateProfile()` and `useChangePassword()` hooks
- `apps/web/src/lib/schemas.ts` — Added profile and password form schemas
- `apps/web/messages/en.json` — Added `profile` namespace
- `apps/web/messages/es.json` — Added `profile` namespace (Spanish)

### Tests
- `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Application/Users/Commands/UpdateProfileCommandHandlerTests.cs` — New: 7 unit tests
- `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Application/Users/Commands/ChangePasswordCommandHandlerTests.cs` — New: 7 unit tests
- `services/orgs-api/tests/SaasTemplate.OrgsApi.IntegrationTests/Api/AuthEndpointTests.cs` — Added 10 integration tests
- `apps/web/src/components/profile/__tests__/ProfileForm.test.tsx` — New: 7 frontend tests
- `apps/web/src/components/profile/__tests__/ChangePasswordForm.test.tsx` — New: 6 frontend tests
- `apps/web/src/components/layout/__tests__/AppHeader.test.tsx` — Updated test for name display

## Tests

### Unit Tests (14 new)
- `UpdateProfileCommandHandlerTests`:
  - Valid command returns updated profile
  - Null name clears name
  - Whitespace name clears name
  - SaveChanges is called
  - User not found throws NotFoundException
  - User not found does not save
  - CancellationToken propagated

- `ChangePasswordCommandHandlerTests`:
  - Valid credentials changes password
  - Valid credentials saves changes
  - User not found throws NotFoundException
  - Wrong password throws UnauthorizedAccessException
  - Wrong password does not hash or save
  - User not found does not call password hasher
  - CancellationToken propagated

### Integration Tests (10 new)
- `AuthEndpointTests`:
  - PATCH /auth/me authenticated returns 200
  - PATCH /auth/me unauthenticated returns 401
  - PATCH /auth/me with null name clears name
  - PATCH /auth/me persists changes (verified via GET /auth/me)
  - PUT /auth/password with valid credentials returns 200
  - PUT /auth/password allows login with new password
  - PUT /auth/password with wrong current password returns 401
  - PUT /auth/password without CSRF returns 403
  - PUT /auth/password unauthenticated returns 401
  - PUT /auth/password with short new password returns 400

### Frontend Tests (13 new)
- `ProfileForm.test.tsx`:
  - Renders loading state
  - Renders form with pre-filled data
  - Shows email as disabled
  - Shows profile title
  - Submits successfully with success toast
  - Shows error toast on failure
  - Shows error state on load failure

- `ChangePasswordForm.test.tsx`:
  - Renders password form fields
  - Shows change password title
  - Validates empty current password
  - Validates short new password
  - Submits successfully with success toast
  - Shows error toast on failure

### Results
- orgs-api: 182 passed (112 unit + 70 integration)
- web: 206 passed (17 test files)

## Security
- No secrets committed.
- Current password verified via BCrypt before allowing password change.
- CSRF protection on password change endpoint.
- Authorization enforced: both endpoints require authentication (`[Authorize]`).
- Input validation via FluentValidation (password min length, name max length).
- Zod validation on frontend for client-side validation.
- New password hashed with BCrypt before storage.

## Checklist
- [x] Task scope matches `docs/tasks/0024-user-profile.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed
