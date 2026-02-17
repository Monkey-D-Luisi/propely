# Task: 0017-authentication-jwt

## Metadata
- ID: 0017
- Type: Standard
- Status: DOING
- Owner: Agent
- Created: 2026-02-03
- Related docs:
  - Walkthrough: `docs/walkthroughs/0017-authentication-jwt.md`

## Goal
Secure the API using JWT Bearer authentication and ensure implementation of user ownership in the domain.

## Context
Currently, the API is open and unsecured. We need to implement standard JWT authentication to protect the endpoints and associate Work Items with specific users (audit/ownership). We will use `dotnet user-jwts` for a simplified local development experience without needing an external IDP immediately.

## Scope
### In scope
- Configure `JwtBearer` authentication in `Program.cs`.
- Apply `[Authorize]` attribute to all Work Item endpoints.
- Update Swagger/OpenAPI configuration to support `Bearer` token input.
- Add `UserId` (Guid) to the `WorkItem` entity.
- Extract `UserId` from `ClaimsPrincipal` in Controllers and pass to Commands.
- Update `CreateWorkItemCommand` and `WorkItem` factory methods to accept `UserId`.

### Out of scope
- Role-based access control (RBAC) - simple authentication for now.
- External Identity Provider integration (e.g., Auth0, Azure AD) - utilizing local dev tools.

## Requirements
- R1: All API endpoints (except health checks/swagger) must require valid JWT.
- R2: Swagger UI must provide a way to input Bearer tokens.
- R3: Work Items must be associated with the user who created them.
- R4: Local development must be supported via `dotnet user-jwts`.

## Acceptance Criteria
- [ ] Unauthenticated requests to `/v1/work-items` return 401 Unauthorized.
- [ ] Authenticated requests with a valid token succeed.
- [ ] Swagger UI allows inputting Bearer token and successfully calls secured endpoints.
- [ ] `UserId` is persisted in the database for new Work Items.
- [ ] Local development workflow using `dotnet user-jwts` is documented in `QUICKSTART.md`.

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Proposed Approach (high-level)
1.  **Infrastructure**: Add JWT Bearer Auth service configuration in `SaasTemplate.AiApi.Api`, utilizing `dotnet user-jwts` for secrets/config.
2.  **API**: Configure Swagger `AddSecurityDefinition` and `AddSecurityRequirement`. Apply global or controller-level `[Authorize]`.
3.  **Domain**: Modify `WorkItem` entity to include `UserId`. Update `WorkItem.Create` factory.
4.  **Application**: Update Commands to include `UserId`. Update Handlers.
5.  **Presentation**: Extract `UserId` from `HttpContext.User` in the Controller and pass it to the Mediator.

## Implementation Steps
1.  Run `dotnet user-jwts init`.
2.  Install `Microsoft.AspNetCore.Authentication.JwtBearer`.
3.  Configure Auth in `Program.cs`.
4.  Configure Swagger in `DependencyInjection.cs`.
5.  Add `UserId` to `WorkItem` entity & EF Configuration.
6.  Create migration for `UserId`.
7.  Update Commands (`CreateWorkItemCommand`) and Handlers.
8.  Update Controller to enforce `[Authorize]` and extract User.
9.  Update Read Model to include `UserId` (projector update).

## Files to Create / Modify
- `src/SaasTemplate.AiApi.Api/Program.cs`
- `src/SaasTemplate.AiApi.Api/Controllers/WorkItemsController.cs`
- `src/SaasTemplate.AiApi.Api/DependencyInjection.cs` (Swagger config)
- `src/SaasTemplate.AiApi.Domain/WorkItems/WorkItem.cs`
- `src/SaasTemplate.AiApi.Application/WorkItems/Commands/CreateWorkItem/CreateWorkItemCommand.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Configurations/WorkItemConfiguration.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemEventProjector.cs`

## Testing Plan
- Unit tests: Update Domain/Handler tests to account for `UserId`.
- Integration tests: Update Controller tests to mock Auth or use valid tokens.
- Manual verification: Use Swagger with a generated token.

## Security & Privacy
- `UserId` is PII (potentially), but here it's just a GUID.
- Ensure JWT validation checks signature and expiration.

## Observability
- Logs should include `UserId` in the correlation scope (Audit).

## Rollback Plan
- Revert commit.
- Rollback DB migration.

## Definition of Done Checklist
- [ ] Acceptance criteria met
- [ ] Build passes
- [ ] Tests added/updated and pass
- [ ] Formatting/analyzers pass
- [ ] No secrets committed
- [ ] Walkthrough updated
