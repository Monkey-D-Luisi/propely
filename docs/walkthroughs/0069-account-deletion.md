# Walkthrough: 0069-account-deletion

## Task Reference
- Task: `docs/tasks/0069-account-deletion.md`
- Walkthrough: `docs/walkthroughs/0069-account-deletion.md`
- Branch/PR: `feat/0069-account-deletion`
- Date: `2026-02-14`

## Summary
Implemented a complete self-service account deletion flow from backend endpoint through frontend UI. The `DELETE /auth/me` endpoint accepts password confirmation, cascades soft-deletion through memberships and sole-owner organizations, cancels Stripe subscriptions, sends a confirmation email, and clears the auth cookie. The frontend adds a "Danger zone" section to the profile page with a modal requiring both password and typed "DELETE" confirmation.

## Context
- Background: The `User.SoftDelete()` domain method and `ISoftDeletable` infrastructure already existed but had no consumer.
- Problem statement: Account deletion is both a GDPR requirement and a basic SaaS expectation. No endpoint, command handler, or UI existed.
- Constraints: Must handle cascade deletion for sole-owner orgs, Stripe subscription cancellation, and work with the existing auth cookie-based session system.

## Decisions & Trade-offs
- **Decision: Sole-owner orgs are cascade-deleted**
  - Options considered: (a) Block deletion if user owns orgs, (b) Transfer ownership, (c) Cascade delete
  - Why this choice: Option (c) is the simplest and most user-friendly. Transfer ownership requires picking a target user which adds UI complexity. Blocking deletion would frustrate users.
  - Consequences: Other members of a sole-owner org lose access. This is documented in the modal warning text.

- **Decision: Confirmation requires both password AND typed "DELETE"**
  - Why: Password prevents CSRF-only attacks. Typed confirmation prevents accidental clicks. Both together provide strong protection for an irreversible action.

- **Decision: Email sent after DB commit (best-effort)**
  - Why: The email is a notification, not a gate. If email fails, the deletion still succeeded. This avoids failing the whole operation due to an email delivery issue.

- **Decision: Stripe subscription cancellation is best-effort with logging**
  - Why: If Stripe API is temporarily unavailable, we still want the account deletion to succeed. The subscription will eventually be cancelled by Stripe when payment fails. Warning log indicates manual follow-up may be needed.

## Implementation Notes
- Key changes:
  - Added `CancelSubscriptionAsync` to `IPaymentService` interface + both implementations
  - Added `SendAccountDeletionConfirmationEmailAsync` to `IEmailService` interface + implementation
  - Created `DeleteAccountCommand` + `DeleteAccountCommandHandler` (CQRS pattern)
  - Added `DELETE /auth/me` endpoint to `AuthController`
  - Created `DeleteAccountSection` component following `DeleteOrgSection` pattern
  - Added `useDeleteAccount` hook
  - Full i18n support (EN/ES)
- Edge cases handled:
  - User not found (404)
  - Wrong password (401)
  - Multiple org memberships with mixed roles
  - Sole-owner orgs with subscriptions
  - Billing disabled (NoOpPaymentService)
  - Email delivery failure (logged, not thrown)
- Known limitations:
  - No cooling-off period (out of scope per task spec)
  - No hard delete / data purge (soft delete only)

## Data / Schema / Migrations
- No DB schema changes needed — the `User.SoftDelete()` method and `ISoftDeletable` query filter were already in place.

## Commands Run
```bash
dotnet build services/orgs-api/SaasTemplate.OrgsApi.sln
dotnet build services/ai-api/SaasTemplate.AiApi.sln
cd apps/web && npm run build
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln --filter "FullyQualifiedName~UnitTests|FullyQualifiedName~ArchitectureTests"
cd apps/web && npm test
```

## Files Changed
### New files
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Commands/DeleteAccount/DeleteAccountCommand.cs` — MediatR command
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Users/Commands/DeleteAccount/DeleteAccountCommandHandler.cs` — Handler with cascade logic
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Dtos/DeleteAccountRequest.cs` — API DTO
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Validators/DeleteAccountRequestValidator.cs` — FluentValidation
- `apps/web/src/components/profile/DeleteAccountSection.tsx` — Danger zone + modal component
- `services/orgs-api/tests/SaasTemplate.OrgsApi.UnitTests/Application/Users/Commands/DeleteAccountCommandHandlerTests.cs` — 8 unit tests

### Modified files
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Billing/Interfaces/IPaymentService.cs` — Added `CancelSubscriptionAsync`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Billing/StripePaymentService.cs` — Implemented cancellation
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Billing/NoOpPaymentService.cs` — No-op implementation
- `services/orgs-api/src/SaasTemplate.OrgsApi.Application/Common/Interfaces/IEmailService.cs` — Added `SendAccountDeletionConfirmationEmailAsync`
- `services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/Services/EmailService.cs` — Email implementation
- `services/orgs-api/src/SaasTemplate.OrgsApi.Api/Controllers/AuthController.cs` — Added DELETE /auth/me endpoint
- `apps/web/src/app/[locale]/profile/page.tsx` — Added DeleteAccountSection
- `apps/web/src/hooks/orgs.ts` — Added useDeleteAccount hook
- `apps/web/messages/en.json` — Added deleteAccount i18n keys
- `apps/web/messages/es.json` — Added deleteAccount i18n keys (Spanish)

## Tests
### Unit
- 8 new tests in `DeleteAccountCommandHandlerTests`:
  - Soft-delete user with valid password
  - Save changes after deletion
  - Send confirmation email
  - Throw NotFoundException when user not found
  - Throw UnauthorizedAccessException for wrong password
  - No save/email on wrong password
  - Cascade-delete sole-owner org (including invitations)
  - Only delete membership for non-sole-owner org
  - Cancel Stripe subscription for sole-owner org
- How to run: `dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln --filter "DeleteAccount"`

### Manual
- Profile page danger zone renders correctly
- Modal opens with password + "DELETE" confirmation
- Confirm button disabled until both fields are filled
- Successful deletion logs user out and redirects to landing page

## Observability
- Security log: `Security: Failed account deletion attempt for user {UserId}`
- Security log: `Security: Account deleted for user {UserId} ({Email})`
- Warning log: Stripe subscription cancellation failure with subscription ID
- Debug log: Account deletion confirmation email sent

## Security
- Validation: Password confirmation required (prevents CSRF-only attacks)
- AuthN/AuthZ impact: Endpoint is [Authorize] + CSRF validated. Auth cookie is cleared after deletion.
- Sensitive data handling: Password is verified against stored hash, never logged. Soft delete preserves data for potential audit/compliance needs.

## Follow-ups / Backlog
- [ ] GDPR data export endpoint (separate task)
- [ ] Cooling-off period / undo deletion (if needed)
- [ ] Hard delete / data purge job (for GDPR "right to erasure" beyond soft delete)

## Checklist
- [x] Task scope matches `docs/tasks/0069-account-deletion.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
