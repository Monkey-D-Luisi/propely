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
dotnet test services/ai-api/SaasTemplate.AiApi.sln
dotnet test services/orgs-api/SaasTemplate.OrgsApi.sln

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
