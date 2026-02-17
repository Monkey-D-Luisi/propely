# Task 0009 — Redis Caching Integration

## Metadata
- Task ID: 0009
- Epic: Work Item Management
- Status: DONE
- Dependencies: 0008 (Event Consumer & Read Model) ✓
- Created: 2026-01-29

## Goal
Add Redis caching to the read path for improved query performance.

## Context
The Event Consumer (Task 0008) populates a read model for efficient queries. This task adds Redis caching between the API and the read model to reduce database load and improve response times.

## Scope

### In Scope
- Create `ICacheService` interface in Application layer
- Implement `RedisCacheService` in Infrastructure layer
- Add cache-aside pattern to `GetWorkItemByIdQueryHandler`
- Configure Redis connection from environment variables
- Add cache invalidation in projector when events are processed

### Out of Scope
- Cache warming strategies
- Distributed cache patterns for multiple nodes
- Cache compression

## Requirements

### Functional
1. **ICacheService Interface**: Abstract cache operations (Get, Set, Remove)
2. **RedisCacheService**: Redis implementation with proper serialization
3. **Cache-aside in Query Handler**:
   - Check cache first: key `workitem:{id}`
   - On miss: query DB, cache result with 5-minute TTL
   - Return cached or fresh data
4. **Cache Invalidation**: Projector removes cached items when events are processed

### Non-Functional
- Redis connection string from `Redis:ConnectionString`
- Graceful fallback if Redis unavailable
- TTL: 5 minutes (configurable)

## Acceptance Criteria
- [ ] Cache hit returns data without DB query
- [ ] Cache miss queries DB and populates cache
- [ ] TTL is 5 minutes
- [ ] Cache keys follow naming convention (`workitem:{guid}`)
- [ ] Projector invalidates cache on event

## Constraints
- Must work with existing Docker Compose setup (Redis container)
- Cannot break existing API functionality
- Must maintain backward compatibility

## Approach

### Implementation Order
1. **Infrastructure**: RedisConfiguration, RedisCacheService
2. **Application**: ICacheService interface
3. **Integration**: Modify GetWorkItemByIdQueryHandler
4. **Projector Update**: Add cache invalidation
5. **Testing**: Integration tests with Redis Testcontainer

## Implementation Steps

### Phase 1: Infrastructure (Redis)
- [ ] Create `RedisConfiguration.cs` in Infrastructure/Caching/Configuration
- [ ] Create `RedisCacheService.cs` implementing ICacheService
- [x] Add Redis NuGet package (StackExchange.Redis)
- [ ] Register in DI

### Phase 2: Application Layer
- [ ] Create `ICacheService.cs` interface
- [ ] Define GetAsync<T>, SetAsync<T>, RemoveAsync methods

### Phase 3: Query Handler Integration
- [ ] Modify GetWorkItemByIdQueryHandler to use cache
- [ ] Implement cache-aside pattern

### Phase 4: Projector Cache Invalidation
- [ ] Add ICacheService to WorkItemProjectorService
- [ ] Remove cache key when projecting WorkItemCreated event

### Phase 5: Testing
- [ ] Add Redis to Docker Compose (already exists)
- [ ] Add integration tests for cache scenarios

## Files to Create/Modify

### New Files
- `src/SaasTemplate.AiApi.Application/Common/Interfaces/ICacheService.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Caching/Configuration/RedisConfiguration.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Caching/RedisCacheService.cs`
- `tests/SaasTemplate.AiApi.IntegrationTests/Caching/RedisCacheServiceTests.cs`

### Modified Files
- `src/SaasTemplate.AiApi.Application/WorkItems/Queries/GetWorkItemByIdQueryHandler.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemProjectorService.cs`
- `src/SaasTemplate.AiApi.Api/Program.cs`

## Testing

### Unit Tests
- Query handler with mocked cache (hit/miss scenarios)

### Integration Tests
- RedisCacheServiceTests: Set/Get/Remove operations
- Cache hit scenario (no DB query)
- Cache miss and population
- TTL expiration behavior

## Security Considerations
- Redis connection string from environment variables
- No sensitive data (only WorkItem IDs and metadata)

## Definition of Done
- [ ] Acceptance criteria met
- [ ] Build passes
- [ ] Tests added/updated and pass
- [ ] Formatting/analyzers pass
- [ ] No secrets committed
- [ ] Walkthrough updated
