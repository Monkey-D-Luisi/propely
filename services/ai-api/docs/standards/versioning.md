# Versioning Standards

## API Versioning

### Strategy
URL path versioning: `/v1/`, `/v2/`

### Rules
- New versions for breaking changes only
- Old versions supported for minimum 6 months
- Breaking changes: removed fields, changed types, removed endpoints

### Non-Breaking Changes (no version bump)
- Adding optional fields
- Adding new endpoints
- Adding optional query parameters

## Event Versioning

### Schema
Events include version suffix: `WorkItemCreatedV1`

### Envelope
```json
{
  "eventType": "WorkItemCreatedV1",
  "schemaVersion": 1,
  ...
}
```

### Evolution Rules
- Adding optional fields: same version, increment `schemaVersion`
- Removing fields: new version (`V2`)
- Changing field types: new version (`V2`)

### Consumer Compatibility
- Consumers must handle unknown fields (ignore)
- Consumers should support N-1 versions minimum

## Database Migrations

### Naming

EF Core migrations use the following format:
```
YYYYMMDDHHMMSS_Description.cs
20260128215228_InitialCreate.cs
```

Generated via:
```bash
dotnet ef migrations add <MigrationName> \
  --project services/ai-api/src/SaasTemplate.AiApi.Infrastructure \
  --startup-project services/ai-api/src/SaasTemplate.AiApi.Api
```

### Rules
- Forward-only migrations
- Idempotent when possible
- Separate data migrations from schema

### Backward Compatibility
- Expand-contract pattern for breaking changes
- Phase 1: Add new column (nullable)
- Phase 2: Migrate data
- Phase 3: Make non-nullable
- Phase 4: Remove old column

## Package Versioning

### Semantic Versioning
`MAJOR.MINOR.PATCH`
- MAJOR: Breaking changes
- MINOR: New features, backward compatible
- PATCH: Bug fixes, backward compatible

## Configuration Versioning
- Version configuration schemas
- Document changes in changelog
- Support old config for transition period
