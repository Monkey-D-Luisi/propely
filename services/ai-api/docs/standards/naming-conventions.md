# Naming Conventions

## General Principles
- English only
- Descriptive over short
- Consistent patterns
- Domain language preferred

## Code Naming

### C# Types
| Type | Convention | Example |
|------|------------|---------|
| Class | PascalCase | `WorkItem` |
| Interface | IPascalCase | `IWorkItemRepository` |
| Enum | PascalCase | `WorkItemStatus` |
| Enum value | PascalCase | `InProgress` |
| Method | PascalCase | `CreateWorkItem` |
| Property | PascalCase | `Title` |
| Field (private) | _camelCase | `_repository` |
| Parameter | camelCase | `workItemId` |
| Local variable | camelCase | `newItem` |
| Constant | PascalCase | `MaxTitleLength` |

### Async Methods
```csharp
// Suffix with Async
Task<WorkItem> GetWorkItemAsync(Guid id);
```

### CQRS Naming
```csharp
// Commands: Verb + Noun + Command
CreateWorkItemCommand
UpdateWorkItemCommand
DeleteWorkItemCommand

// Queries: Get + Noun + Query
GetWorkItemByIdQuery
GetAllWorkItemsQuery

// Handlers: Command/Query + Handler
CreateWorkItemCommandHandler
GetWorkItemByIdQueryHandler
```

### Events
```csharp
// Past tense + Version
WorkItemCreatedV1
WorkItemUpdatedV1
WorkItemDeletedV1
```

### DTOs
```csharp
// Request/Response suffix
CreateWorkItemRequest
WorkItemResponse
```

## Database Naming

### Tables
- snake_case, plural
- `work_items`, `outbox_messages`

### Columns
- snake_case
- `created_at_utc`, `work_item_id`

### Indexes
- `idx_<table>_<columns>`
- `idx_work_items_status`

### Constraints
- Primary key: `pk_<table>`
- Foreign key: `fk_<table>_<referenced_table>`
- Unique: `uq_<table>_<columns>`
- Check: `chk_<table>_<description>`

## File Naming

### Source Files
- Match type name: `WorkItem.cs`
- One public type per file

### Documentation
- Lowercase, hyphens: `naming-conventions.md`
- Tasks: `NNNN-title.md`

### Configuration
- Lowercase, hyphens: `appsettings.json`

## API Naming

### Endpoints
- Lowercase, hyphens, plural nouns
- `/v1/work-items`
- `/v1/work-items/{id}`

### Query Parameters
- camelCase
- `?pageSize=10&sortBy=createdAt`

## Message Queue Naming

### Exchanges
- `<domain>.<aggregate>`
- `workitems.events`

### Queues
- `<service>.<purpose>`
- `projector.workitems`

### Routing Keys
- `<event-type>`
- `WorkItemCreatedV1`

## Cache Keys
- Colon-separated, lowercase
- `workitem:{id}`
- `workitems:list:page:{n}`
