# Audit Action: audit-0002-jwt-authentication

## Metadata
- ID: audit-0002
- Type: AuditAction
- Status: DONE
- Owner: Agent
- Created: 2026-01-30
- Priority: P0
- Source:
  - Executive summary: `docs/audits/2026-01-30-executive-summary.md`
  - Action plan item: "Implement JWT authentication + policy-based authorization; add auth middleware"
- Dependencies:
  - audit-0001 (Remove committed credentials) - completed in PR #29
- Related docs:
  - Walkthrough: `docs/walkthroughs/audit-0002-jwt-authentication.md`
  - Security baseline: `docs/standards/security-baseline.md`
  - Security audit: `docs/audits/2026-01-30-security.md` (Finding F-01)

## Goal
Implement JWT bearer authentication and policy-based authorization per the security baseline, with anonymous access allowed only in development mode.

## Context
The security audit (F-01) identified that all API endpoints are effectively anonymous. The security baseline requires:
- JWT Bearer tokens with external identity provider
- Policy-based authorization (not magic role strings)
- Anonymous access allowed locally with explicit opt-in flag
- Fail closed (deny by default)

## Scope
### In scope
- Add JWT bearer authentication configuration
- Register authorization policies per security baseline
- Add `UseAuthentication()` and `UseAuthorization()` middleware
- Apply `[Authorize]` policies to controller endpoints
- Implement development-mode anonymous access via configuration flag
- Add JWT configuration section to appsettings

### Out of scope
- External identity provider setup (issuer configuration is placeholder)
- HTTPS/HSTS enforcement (P1 action - audit-0003)
- User management or claims enrichment

## Requirements
- R1: JWT authentication must validate issuer, audience, and lifetime
- R2: Policies defined centrally (CanCreateWorkItem, CanReadWorkItem)
- R3: Development mode allows anonymous access via `Security:AllowAnonymous` flag
- R4: Production must deny by default (fail closed)
- R5: Integration tests must continue to work

## Acceptance Criteria
- [x] JWT bearer authentication configured in Program.cs
- [x] Authorization policies registered per security baseline
- [x] Middleware pipeline includes UseAuthentication/UseAuthorization
- [x] Controller endpoints protected with [Authorize] policies
- [x] Development mode allows anonymous access
- [x] Build passes
- [x] All tests pass (79 tests)

## Constraints
- C1: Must not break existing integration tests
- C2: Must align with security baseline patterns
- C3: Development experience must remain smooth (anonymous access in dev)

## Implementation Steps
1. Add Security configuration section to appsettings.json and appsettings.Development.json
2. Create authorization policies configuration in Program.cs
3. Add JWT bearer authentication services
4. Implement conditional anonymous access for development
5. Add UseAuthentication/UseAuthorization to middleware pipeline
6. Apply [Authorize] policies to controller endpoints
7. Update integration tests if needed
8. Run build and tests

## Testing Plan
- Unit tests: None required (infrastructure configuration)
- Integration tests: Verify existing tests pass (they should use anonymous dev mode)
- Manual checks: Verify authentication is required in non-dev mode

## Definition of Done
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests pass (79 tests)
- [x] No secrets committed
- [x] Walkthrough updated
