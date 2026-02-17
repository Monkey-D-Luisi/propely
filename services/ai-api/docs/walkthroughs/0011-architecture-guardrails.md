# Walkthrough: 0011-architecture-guardrails

## Task Reference
- Task: `docs/tasks/0011-architecture-guardrails.md`
- Walkthrough: `docs/walkthroughs/0011-architecture-guardrails.md`
- Branch/PR: `feat/0011-architecture-guardrails`
- Date: 2026-02-02

## Summary
Implemented automated `NetArchTest` rules to mechanistically enforce Clean Architecture boundaries. The build will now fail if architectural principles are violated.

## Context
- Background: We want to ensure no accidental dependency leaks (e.g., Domain -> Infrastructure).
- Problem statement: Manual reviews are error-prone.
- Constraints: Must be part of `dotnet test`.

## Decisions & Trade-offs
- **Decision:** Use `NetArchTest.Rules`.
  - Options considered: ArchUnitNET, manual reflection.
  - Why this choice: Simple fluent API, widely used in .NET ecosystem.
  - Consequences / risks: Dependency on 3rd party lib (low risk).

## Implementation Notes
- Key changes: New test project `SaasTemplate.AiApi.ArchitectureTests`. It references all other layers to inspect them.
- Edge cases handled: Verified `IDomainEvent` implementations are sealed.

## Commands Run
```bash
dotnet new xunit -n SaasTemplate.AiApi.ArchitectureTests -o tests/SaasTemplate.AiApi.ArchitectureTests
dotnet sln add tests/SaasTemplate.AiApi.ArchitectureTests/SaasTemplate.AiApi.ArchitectureTests.csproj
dotnet add tests/SaasTemplate.AiApi.ArchitectureTests/SaasTemplate.AiApi.ArchitectureTests.csproj package NetArchTest.Rules
dotnet test tests/SaasTemplate.AiApi.ArchitectureTests/SaasTemplate.AiApi.ArchitectureTests.csproj
```

## Files Changed
- `tests/SaasTemplate.AiApi.ArchitectureTests/SaasTemplate.AiApi.ArchitectureTests.csproj` - New project
- `tests/SaasTemplate.AiApi.ArchitectureTests/ArchitectureTests.cs` - Rules implementation
- `SaasTemplate.AiApi.sln` - Added project

## Tests
### Unit
- What was added/updated: 5 Architecture rules (Domain Isolation, Layer Direction, Handler Naming, Sealed Events).
- How to run: `dotnet test`
- Results: 5 Passed.

## Checklist
- [x] Task scope matches `docs/tasks/0011-architecture-guardrails.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed
