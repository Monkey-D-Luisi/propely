# Task: 0006-presentation-layer

## Metadata
- ID: 0006
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-01-28
- Related docs:
  - Walkthrough: `docs/walkthroughs/0006-presentation-layer.md`
  - Epic: `docs/backlog/work-item-management-epic.md`

## Goal
Expose REST API endpoints for Work Item operations.

## Context
Previous tasks (0003-0005) built the Domain, Application, and Infrastructure layers. This task completes the vertical slice by adding the Presentation layer.

## Scope
### In scope
- Create new `SaasTemplate.AiApi.Api` ASP.NET Core project
- POST /v1/work-items endpoint
- GET /v1/work-items/{id} endpoint
- FluentValidation for request validation
- Security headers middleware
- Rate limiting (100 req/min)
- Integration tests with WebApplicationFactory

### Out of scope
- Authorization/authentication (dev mode only)
- Other CRUD operations (update, delete, list)
- OpenAPI/Swagger documentation

## Requirements
- R1: POST /v1/work-items returns 201 Created with work item data
- R2: POST returns 400 on validation failure
- R3: GET /v1/work-items/{id} returns 200 OK with work item
- R4: GET returns 404 for non-existent ID
- R5: Security headers present in all responses
- R6: Rate limiting active

## Acceptance Criteria
- [x] POST returns 201 with created resource
- [x] POST returns 400 on validation failure
- [x] GET returns 200 with correct data
- [x] GET returns 404 for non-existent ID
- [x] Security headers present in responses
- [x] Rate limiting is active

## Constraints (non-negotiable)
- Clean Architecture layers respected.
- English-only repo content.
- No secrets in repo.
- Update walkthrough.

## Files Created
- `src/SaasTemplate.AiApi.Api/` — New Web API project
- `src/SaasTemplate.AiApi.Api/Controllers/WorkItemsController.cs`
- `src/SaasTemplate.AiApi.Api/Dtos/CreateWorkItemRequest.cs`
- `src/SaasTemplate.AiApi.Api/Dtos/WorkItemResponse.cs`
- `src/SaasTemplate.AiApi.Api/Validators/CreateWorkItemRequestValidator.cs`
- `src/SaasTemplate.AiApi.Api/Middleware/SecurityHeadersMiddleware.cs`
- `tests/SaasTemplate.AiApi.IntegrationTests/Fixtures/ApiWebApplicationFactory.cs`
- `tests/SaasTemplate.AiApi.IntegrationTests/Api/WorkItemsControllerTests.cs`

## Testing Plan
- Unit tests: FluentValidation rules (implicit via integration tests)
- Integration tests: 7 tests covering POST/GET endpoints and security headers
- All 50 solution tests pass

## Definition of Done Checklist
- [x] Acceptance criteria met
- [x] Build passes
- [x] Tests added/updated and pass
- [x] Formatting/analyzers pass
- [x] No secrets committed
- [x] Walkthrough updated
