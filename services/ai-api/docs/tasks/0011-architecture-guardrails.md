# Task: 0011-architecture-guardrails

## Metadata
- ID: 0011
- Type: Standard
- Status: DOING
- Owner: Agent
- Created: 2026-02-02
- Related docs:
  - Walkthrough: `docs/walkthroughs/0011-architecture-guardrails.md`

## Goal
Implement automated architecture tests to enforce Clean Architecture layer dependencies and naming conventions, ensuring that no developer (human or AI) can accidentally introduce architectural violations.

## Context
As the project grows, it becomes easier to accidentally reference `Infrastructure` from `Domain` or `Api` from `Application`. To make this template "Agent-Ready," we need mechanistic guardrails that fail the build if the architecture is violated.

## Scope
### In scope
- Create `tests/SaasTemplate.AiApi.ArchitectureTests` project.
- Install `NetArchTest.Rules`.
- Implement Dependency Rules:
  - Domain independent of all.
  - Application depends only on Domain.
  - Infrastructure depends on Application/Domain.
  - Api depends on Application/Infrastructure.
- Implement Convention Rules example (Handlers naming).

### Out of scope
- Refactoring existing code (unless tests reveal violations, which they shouldn't yet).

## Requirements
- R1: Build must fail if Domain depends on Infrastructure.
- R2: Build must fail if Application depends on Infrastructure.
- R3: Test suite must extendable.

## Acceptance Criteria
- [ ] `tests/SaasTemplate.AiApi.ArchitectureTests` project exists in SLN.
- [ ] Tests pass for current valid architecture.
- [ ] Tests would fail if I introduced a violation (verified manually during dev).
- [ ] CI pipeline runs these tests (via `dotnet test`).

## Proposed Approach (high-level)
1.  Create xUnit project.
2.  Use `NetArchTest.Rules` to define policies.
3.  Add reference to all source projects to analyze them.

## Files to Create / Modify
- `tests/SaasTemplate.AiApi.ArchitectureTests/SaasTemplate.AiApi.ArchitectureTests.csproj`
- `tests/SaasTemplate.AiApi.ArchitectureTests/ArchitectureTests.cs`

## Testing Plan
- Unit tests: The project itself IS a test suite.
- Manual verification: Temporarily add a bad reference to verify it fails.

## Definition of Done Checklist
- [ ] Acceptance criteria met
- [ ] Build passes
- [ ] Tests added/updated and pass
- [ ] Formatting/analyzers pass
- [ ] No secrets committed
- [ ] Walkthrough updated
