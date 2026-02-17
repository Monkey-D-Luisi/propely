# Architecture Standards

## Clean Architecture Layers

### Layer Dependencies (strict)
```
Presentation → Application → Domain
                    ↓
             Infrastructure
```

- Domain: no dependencies on other layers
- Application: depends on Domain only
- Infrastructure: depends on Application (for interfaces) and Domain
- Presentation: depends on Application (and transitively Domain)

### Domain Layer
Location: `src/<Service>.Domain/`

Contains:
- Entities (identity + behavior)
- Value Objects (immutable, equality by value)
- Domain Events (facts)
- Domain Services (stateless operations across aggregates)
- Repository interfaces
- Domain exceptions

Rules:
- No framework dependencies
- No persistence logic
- No DTOs
- Rich behavior, not anemic

### Application Layer
Location: `src/<Service>.Application/`

Contains:
- Commands and Queries (CQRS)
- Command/Query Handlers
- Application Services (orchestration)
- Port interfaces (e.g., `IEmailService`, `IPaymentGateway`)
- DTOs for inter-layer communication (if needed)

Rules:
- Use cases are explicit (one handler per use case)
- No direct infrastructure dependencies
- Coordinates, doesn't contain business rules

### Infrastructure Layer
Location: `src/<Service>.Infrastructure/`

Contains:
- Repository implementations
- Database context and migrations
- External service clients (HTTP, gRPC)
- Message bus implementations
- Cache implementations
- File storage implementations

Rules:
- Implements interfaces from Application
- May reference Domain types
- Contains all framework-specific code

### Presentation Layer
Location: `src/<Service>.Api/`

Contains:
- Controllers / Endpoints
- Request/Response DTOs
- Validation (FluentValidation)
- Middleware
- Filters
- Auth policies

Rules:
- Thin controllers (delegate to handlers)
- Validate input at the edge
- Transform to/from DTOs
- Handle HTTP concerns only

## CQRS Guidelines

### Commands
- Represent intent to change state
- Return void or simple result (not entities)
- One handler per command
- May publish events

### Queries
- Read-only, no side effects
- Return DTOs, not entities
- May use separate read model
- Optimized for read scenarios

## DDD Tactical Patterns

### Aggregates
- Define consistency boundaries
- One aggregate root per aggregate
- Load/save as a unit
- Small aggregates preferred

### Entities
- Have identity
- Mutable (controlled)
- Contain behavior

### Value Objects
- No identity (equality by value)
- Immutable
- Self-validating

### Domain Events
- Named in past tense
- Immutable
- Include all relevant data
- Version suffix: `V1`, `V2`

## Monorepo Project Structure

```
services/<service-name>/
├── src/
│   ├── SaasTemplate.<Service>.Domain/
│   │   └── <Feature>/
│   ├── SaasTemplate.<Service>.Application/
│   │   └── <Feature>/
│   │       ├── Commands/
│   │       ├── Queries/
│   │       └── Interfaces/
│   ├── SaasTemplate.<Service>.Infrastructure/
│   │   ├── Persistence/
│   │   ├── Messaging/
│   │   └── Services/
│   └── SaasTemplate.<Service>.Api/
│       ├── Controllers/
│       ├── Dtos/
│       └── Validators/
└── tests/
    ├── SaasTemplate.<Service>.UnitTests/
    └── SaasTemplate.<Service>.IntegrationTests/
```

## Anti-Patterns to Avoid
- Anemic domain model (logic outside entities)
- God services (do one thing well)
- Leaky abstractions (infrastructure in domain)
- Over-engineering (YAGNI applies)
- Circular dependencies
- Service locator pattern
