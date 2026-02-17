# PR Review: <PR Title>

## PR Information
- PR: #<number>
- Author: @<author>
- Reviewer: @<reviewer>
- Date: YYYY-MM-DD

## Summary
Brief description of what this PR does.

## Review Checklist

### Code Quality
- [ ] Code is readable and self-documenting
- [ ] No unnecessary complexity
- [ ] No code duplication
- [ ] Naming is clear and consistent
- [ ] Comments explain "why" not "what"

### Architecture (Clean Architecture + DDD)
- [ ] Correct layer placement (Domain/Application/Infrastructure/Presentation)
- [ ] No layer violations (inner layers don't depend on outer)
- [ ] Domain logic in Domain layer
- [ ] Use cases in Application layer
- [ ] Infrastructure concerns isolated
- [ ] DTOs at API boundary only

### SOLID Principles
- [ ] Single Responsibility: each class has one reason to change
- [ ] Open/Closed: extensible without modification
- [ ] Liskov Substitution: subtypes are substitutable
- [ ] Interface Segregation: no forced unused dependencies
- [ ] Dependency Inversion: depend on abstractions

### Security
- [ ] No secrets or credentials in code
- [ ] Input validation present
- [ ] SQL injection prevented (parameterized queries)
- [ ] Authorization checks in place
- [ ] Sensitive data not logged
- [ ] HTTPS enforced where applicable

### Testing
- [ ] Unit tests cover core logic
- [ ] Integration tests for critical paths
- [ ] Edge cases considered
- [ ] Tests are deterministic
- [ ] No test interdependencies

### Observability
- [ ] Appropriate logging present
- [ ] Correlation IDs propagated
- [ ] No PII in logs
- [ ] Metrics for critical operations

### Documentation
- [ ] Walkthrough updated
- [ ] Public APIs documented
- [ ] Complex logic explained
- [ ] Breaking changes documented

## Findings

### Critical (must fix)
- None

### Major (should fix)
- None

### Minor (nice to have)
- None

### Questions
- None

## Verdict
- [ ] Approved
- [ ] Approved with minor changes
- [ ] Request changes

## Notes
Additional context or follow-up items.
