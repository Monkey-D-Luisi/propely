# Testing Standards

## Test Types

### Unit Tests
- Test single units in isolation
- Mock all dependencies
- Fast (<100ms per test)
- Location: `tests/<Project>.UnitTests/`

### Integration Tests
- Test component interactions
- Use real dependencies (via containers)
- Location: `tests/<Project>.IntegrationTests/`

## Naming Conventions

### Test Classes
```
<ClassUnderTest>Tests
```

### Test Methods
```
<MethodName>_<Scenario>_<ExpectedResult>
```

Examples:
- `CreateInvitation_ValidInput_ReturnsSuccess`
- `RegisterUser_DuplicateEmail_ThrowsConflict`
- `GetCurrentUser_InvalidId_ReturnsNull`

## Test Structure (AAA Pattern)

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var sut = CreateSystemUnderTest();
    var input = CreateValidInput();

    // Act
    var result = await sut.ExecuteAsync(input);

    // Assert
    result.Should().NotBeNull();
    result.Id.Should().NotBeEmpty();
}
```

## Frameworks & Libraries
- xUnit: test framework
- FluentAssertions: assertions
- NSubstitute: mocking
- Testcontainers: integration test infrastructure
- Bogus: test data generation

## Best Practices

### General
- One assertion concept per test (multiple asserts OK if same concept)
- Tests must be independent (no shared state)
- Tests must be deterministic
- No logic in tests (no if/loops)
- Prefer factory methods over constructors in tests

### Mocking
- Mock only direct dependencies
- Don't mock what you don't own (wrap first)
- Verify interactions sparingly
- Prefer state verification over interaction verification

### Test Data
- Use builders or factories
- Avoid magic values (use constants or builders)
- Generate realistic data with Bogus

### Integration Tests
- Use `WebApplicationFactory` for API tests
- Use Testcontainers for databases
- Clean up after each test
- Parallelize where safe

## Coverage Guidelines
- Domain layer: >90%
- Application layer: >80%
- Infrastructure layer: >60%
- Presentation layer: >50%

## Running Tests

```bash
# All tests for a service
dotnet test services/ai-api/Propely.AiApi.sln
dotnet test services/orgs-api/Propely.OrgsApi.sln
dotnet test services/properties-api/Propely.PropertiesApi.sln
dotnet test services/contacts-api/Propely.ContactsApi.sln
dotnet test services/appointments-api/Propely.AppointmentsApi.sln
dotnet test services/publishing-api/Propely.PublishingApi.sln

# Unit tests only
dotnet test --filter "Category=Unit"

# Integration tests only
dotnet test --filter "Category=Integration"
```

## CI Requirements
- All tests must pass before merge
- No flaky tests (fix or quarantine)
- Coverage reports generated
- Integration tests run in isolated containers

## E2E Tests (Playwright)
- Location: `apps/web/e2e/`
- Framework: Playwright Test (Chromium)
- Config: `apps/web/playwright.config.ts`
- Base URL: `http://localhost:3000` (override with `PLAYWRIGHT_BASE_URL`)

### E2E Test Types
| Type | Location | Purpose |
|------|----------|---------|
| Functional | `apps/web/e2e/*.spec.ts` | User flows (auth, navigation, CRUD) |
| Visual regression | `apps/web/e2e/visual/*.spec.ts` | Screenshot comparison against baselines |

### Visual Regression Tests
- Use `await expect(page).toHaveScreenshot('<name>.png')` for pixel comparison
- Baselines stored in `apps/web/e2e/visual/*.spec.ts-snapshots/`
- Update baselines: `npx playwright test --update-snapshots`
- Tolerance: `maxDiffPixelRatio: 0.01` (1%) for layout, `0.05` (5%) for dynamic content
- Always test at Desktop Chrome viewport (1280x720) for consistency

### E2E Strategies
1. **Real backend** (auth, org flows) — requires Docker stack running
2. **Mocked API** (properties, contacts) — uses `page.route()` for API interception

### Running E2E Tests
```bash
cd apps/web

# All E2E tests
npx playwright test

# Visual regression only
npx playwright test e2e/visual/

# Update visual baselines
npx playwright test --update-snapshots

# Interactive UI mode
npx playwright test --ui
```

## Agent Visual Validation (MCP-based)

During development, the agent uses MCP tools for real-time visual validation (not part of CI):

### Playwright MCP (`@anthropic-ai/mcp-playwright`)
- **Browser automation**: navigate, click, fill, select, upload, drag & drop
- **Testing assertions** (`--caps=testing`): `browser_verify_text_visible`, `browser_verify_is_visible`, `browser_verify_page_title`
- **Accessibility tree** (`browser_snapshot`): token-efficient page structure (no screenshots needed for element verification)
- **Test generation** (`browser_generate_playwright_test`): captures interactions as reusable Playwright tests
- **Multi-tab, network, console**: full browser introspection

### Chrome DevTools MCP (`@anthropic-ai/mcp-chrome-devtools`)
- **Performance**: Core Web Vitals tracing (LCP, INP, CLS), V8 heap snapshots
- **Accessibility**: ARIA tree inspection, contrast issues
- **Network**: request/response bodies, failed requests, waterfall analysis
- **Console**: source-mapped error stack traces, warning detection
- **Audits**: Lighthouse scores (accessibility target: >90, performance target: >80)
