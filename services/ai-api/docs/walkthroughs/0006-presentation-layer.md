# Walkthrough: 0006-presentation-layer

## Task Reference
- Task: `docs/tasks/0006-presentation-layer.md`
- Date: 2026-01-28

## Summary
Implemented REST API endpoints for Work Item management, completing the presentation layer of the vertical slice. Created `POST /v1/work-items` (201/400) and `GET /v1/work-items/{id}` (200/404) endpoints with FluentValidation, security headers middleware, and rate limiting.

## Decisions & Trade-offs
- **AspNetCoreRateLimit package**: Chosen for in-memory rate limiting (100 req/min). Simple setup, adequate for single-instance deployment.
- **Manual validation dispatch**: Controller manually validates and returns 400 rather than using automatic filter, giving explicit control over error response format.
- **Security headers via middleware**: Custom middleware instead of NuGet package for transparency and minimal dependencies.

## Implementation Notes
- Created new `SaasTemplate.AiApi.Api` project with references to Infrastructure layer
- `Program.cs` registers MediatR, FluentValidation, EF Core, repositories, and rate limiting
- Partial `Program` class for `WebApplicationFactory` integration testing
- `ApiWebApplicationFactory` replaces Postgres with Testcontainers for isolated tests

## Commands Run
```bash
dotnet new webapi -n SaasTemplate.AiApi.Api -o src/SaasTemplate.AiApi.Api --no-openapi --use-controllers
dotnet sln add src/SaasTemplate.AiApi.Api/SaasTemplate.AiApi.Api.csproj --solution-folder src
dotnet build
dotnet test
```

## Files Changed

### New Files
| File | Purpose |
|------|---------|
| `src/SaasTemplate.AiApi.Api/SaasTemplate.AiApi.Api.csproj` | Project with FluentValidation, AspNetCoreRateLimit |
| `src/SaasTemplate.AiApi.Api/Program.cs` | Host configuration with DI and middleware |
| `src/SaasTemplate.AiApi.Api/Controllers/WorkItemsController.cs` | POST/GET endpoints |
| `src/SaasTemplate.AiApi.Api/Dtos/CreateWorkItemRequest.cs` | Request DTO |
| `src/SaasTemplate.AiApi.Api/Dtos/WorkItemResponse.cs` | Response DTO |
| `src/SaasTemplate.AiApi.Api/Validators/CreateWorkItemRequestValidator.cs` | FluentValidation rules |
| `src/SaasTemplate.AiApi.Api/Middleware/SecurityHeadersMiddleware.cs` | Security headers |
| `tests/.../Fixtures/ApiWebApplicationFactory.cs` | Test factory with Testcontainers |
| `tests/.../Api/WorkItemsControllerTests.cs` | 7 integration tests |

### Modified Files
| File | Change |
|------|--------|
| `SaasTemplate.AiApi.sln` | Added Api project |
| `tests/.../SaasTemplate.AiApi.IntegrationTests.csproj` | Added Mvc.Testing + Api reference |

## Tests

### Integration (7 new tests)
- `POST_ReturnsCreated_WithValidRequest`
- `POST_ReturnsBadRequest_WhenTitleEmpty`
- `POST_ReturnsBadRequest_WhenTitleTooLong`
- `POST_ReturnsBadRequest_WhenDescriptionTooLong`
- `GET_ReturnsOk_WhenWorkItemExists`
- `GET_ReturnsNotFound_WhenWorkItemDoesNotExist`
- `Response_ContainsSecurityHeaders`

**Run:** `dotnet test --filter "WorkItemsControllerTests"`

**Result:** All 50 solution tests pass

## Security
- Security headers: X-Content-Type-Options, X-Frame-Options, X-XSS-Protection, Referrer-Policy, CSP
- Input validation at API edge (FluentValidation)
- No authentication (dev mode)

## Checklist
- [x] Task scope matches `docs/tasks/0006-presentation-layer.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed
