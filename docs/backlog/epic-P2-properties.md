# Epic P2 -- Properties & Listings

## Overview

Build the core property management domain for Propely, enabling real estate agents to create, manage, and search property listings. Properties are the central entity of the platform -- every subsequent phase (AI Action Engine, Contacts & Leads, Appointments) depends on the property model established here. The properties-api service owns all property domain logic, persistence, media management, and exposes a NuGet SDK client for cross-service consumption.

**Target:** Agents can create property listings with structured data (via JSON Forms), upload media, manage status lifecycle, search and filter properties, and view detailed property pages. All backend services can access property data via the `Propely.PropertiesApi.Client` SDK.

**Portal field reference:** The property model includes fields informed by portal schemas (Kyero v3.9, SpainHouses XSD, Thribee XML) to ensure domain completeness. These are fields that portals require for listings and represent the standard data model for real estate in Spain/Portugal. Portal publication (Phase 6) is deprioritized, but the field model remains comprehensive.

## Service Ownership

| Capability | Service |
|---|---|
| Property domain model, persistence, API | `services/properties-api` |
| Media storage (GCP Cloud Storage) | `services/properties-api` (Infrastructure layer) |
| JSON Schema for property data | `services/properties-api` + `apps/web` |
| NuGet SDK client | `services/properties-api` (Client package) |
| Frontend property UI | `apps/web` |

## Property Model Summary

### Property Types

`Apartment`, `House`, `Villa`, `Penthouse`, `Studio`, `Commercial`, `Land`, `Garage`, `StorageRoom`, `Building`, `Office`

### Operation Types

`Sale`, `Rent`, `SaleOrRent`, `Transfer`, `Vacation`

### Status Lifecycle

```
Draft ──────► Active ──────► Reserved ──────► Sold
  │              │                              Rented
  │              ├──────────────────────────► Archived
  └──────────────────────────────────────────► Archived
```

Valid transitions:
- `Draft` → `Active`, `Archived`
- `Active` → `Reserved`, `Sold`, `Rented`, `Archived`
- `Reserved` → `Active` (release), `Sold`, `Rented`
- `Sold`, `Rented`, `Archived` → terminal (no transitions out)

## Tasks

| # | Title | Status | Dependencies |
|---|---|---|---|
| 2.1 | Property Domain Model | PENDING | 0.3 |
| 2.2 | Property Persistence & Repository | PENDING | 2.1 |
| 2.3 | Property CRUD API | PENDING | 2.2 |
| 2.4 | Property JSON Schema & Validation | PENDING | 2.1 |
| 2.5 | Property Media Management | PENDING | 2.2 |
| 2.6 | Properties-API NuGet SDK Client | PENDING | 2.3, 0.4 |
| 2.7 | Property List UI | PENDING | 2.3 |
| 2.8 | Property Create/Edit Form | PENDING | 2.3, 2.4 |
| 2.9 | Property Detail View | PENDING | 2.3, 2.5 |
| 2.10 | Property Search & Advanced Filters | PENDING | 2.3 |

---

## Task 2.1 -- Property Domain Model

**Status:** PENDING
**Dependencies:** 0.3 (properties-api service scaffold must exist)

### Goal

Define the `Property` aggregate root, value objects, enums, domain events, and business rules that model a real estate listing. This is a pure domain layer with zero framework dependencies.

### Scope

**In scope:**
- `Property` aggregate root entity in `PropertiesApi.Domain`
- Enums: `PropertyType` (11 values), `OperationType` (5 values), `PropertyStatus` (6 values), `EnergyRating` (A-G + Exempt), `Orientation` (N, NE, E, SE, S, SW, W, NW)
- Value objects: `Address` (street, city, province, postalCode, country, provinceCode (INE 2-digit), municipalityCode (INE 5-digit), latitude, longitude), `PropertyFeatures` (bedrooms, bathrooms, builtArea, usableArea, plotArea, floor, orientation, yearBuilt, energyRating, energyConsumption, energyEmissions, hasPool, hasGarden, hasGarage, hasElevator, hasTerrace, airConditioning, heating, furnished, parkingSpaces), `PropertyFinancials` (price, communityFees, ibiTax, catastroReference), `LocalizedText` (es, pt, en, fr, de, nl)
- Domain events: `PropertyCreatedV1`, `PropertyUpdatedV1`, `PropertyStatusChangedV1`, `PropertyDeletedV1`
- Status state machine with transition validation
- Business rules: title required, price >= 0, builtArea >= 0, bedrooms >= 0, valid status transitions only
- `PricePerSqm` calculated property (price / builtArea when both > 0)
- Audit fields: `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`, `PublishedAt`
- Soft-delete support via `ISoftDeletable`

**Out of scope:**
- Persistence/EF Core mapping (task 2.2)
- API endpoints (task 2.3)
- JSON Schema (task 2.4)
- Media management (task 2.5)

### Acceptance Criteria

- [ ] **AC1:** `Property` entity inherits from `Entity` and implements `ISoftDeletable`
- [ ] **AC2:** `Property` has required fields: `Title`, `PropertyType`, `OperationType`, `Status`, `TenantId`, `AgentId`
- [ ] **AC3:** `Property` has optional fields: `Description` (LocalizedText), `Address`, `Features`, `Financials`, `VirtualTourUrl`, `VideoUrl`, `AgencyId`
- [ ] **AC4:** `PropertyType` enum includes all 11 values: `Apartment`, `House`, `Villa`, `Penthouse`, `Studio`, `Commercial`, `Land`, `Garage`, `StorageRoom`, `Building`, `Office`
- [ ] **AC5:** `OperationType` enum includes all 5 values: `Sale`, `Rent`, `SaleOrRent`, `Transfer`, `Vacation`
- [ ] **AC6:** `PropertyStatus` enum includes all 6 values: `Draft`, `Active`, `Reserved`, `Sold`, `Rented`, `Archived`
- [ ] **AC7:** `Property.Create()` factory validates required fields, sets status to `Draft`, raises `PropertyCreatedV1`
- [ ] **AC8:** `Property.ChangeStatus(newStatus)` validates transition (state machine), raises `PropertyStatusChangedV1`, sets `PublishedAt` on first transition to `Active`
- [ ] **AC9:** Invalid status transitions throw `DomainException` with descriptive message
- [ ] **AC10:** `PricePerSqm` returns `price / builtArea` when both > 0, null otherwise
- [ ] **AC11:** `Address` value object validates latitude (-90 to 90) and longitude (-180 to 180) when provided
- [ ] **AC12:** `LocalizedText` value object has `Es`, `Pt`, `En` properties; at least one must be non-empty for title
- [ ] **AC13:** All domain events include `PropertyId`, `TenantId`, `AgentId`, `OccurredAt`
- [ ] **AC14:** Unit tests cover all factory methods, status transitions, validation rules, and calculated properties

### Implementation Steps

1. Create `PropertyType` enum in `Domain/Properties/`
2. Create `OperationType` enum in `Domain/Properties/`
3. Create `PropertyStatus` enum in `Domain/Properties/`
4. Create `EnergyRating` enum in `Domain/Properties/`
5. Create `Orientation` enum in `Domain/Properties/`
6. Create `Address` value object in `Domain/Properties/ValueObjects/`
7. Create `PropertyFeatures` value object in `Domain/Properties/ValueObjects/`
8. Create `PropertyFinancials` value object in `Domain/Properties/ValueObjects/`
9. Create `LocalizedText` value object in `Domain/Properties/ValueObjects/`
10. Create `Property` aggregate root with private constructor, factory methods, status state machine
11. Create domain events in `Domain/Properties/Events/`
12. Create `PropertyValidationException` in `Domain/Properties/Exceptions/`
13. Write comprehensive unit tests for all domain logic

### Files to Create/Modify

**Create:**
- `services/properties-api/src/Propely.PropertiesApi.Domain/Properties/Property.cs`
- `services/properties-api/src/Propely.PropertiesApi.Domain/Properties/PropertyType.cs`
- `services/properties-api/src/Propely.PropertiesApi.Domain/Properties/OperationType.cs`
- `services/properties-api/src/Propely.PropertiesApi.Domain/Properties/PropertyStatus.cs`
- `services/properties-api/src/Propely.PropertiesApi.Domain/Properties/EnergyRating.cs`
- `services/properties-api/src/Propely.PropertiesApi.Domain/Properties/Orientation.cs`
- `services/properties-api/src/Propely.PropertiesApi.Domain/Properties/ValueObjects/Address.cs`
- `services/properties-api/src/Propely.PropertiesApi.Domain/Properties/ValueObjects/PropertyFeatures.cs`
- `services/properties-api/src/Propely.PropertiesApi.Domain/Properties/ValueObjects/PropertyFinancials.cs`
- `services/properties-api/src/Propely.PropertiesApi.Domain/Properties/ValueObjects/LocalizedText.cs`
- `services/properties-api/src/Propely.PropertiesApi.Domain/Properties/Events/PropertyCreatedV1.cs`
- `services/properties-api/src/Propely.PropertiesApi.Domain/Properties/Events/PropertyUpdatedV1.cs`
- `services/properties-api/src/Propely.PropertiesApi.Domain/Properties/Events/PropertyStatusChangedV1.cs`
- `services/properties-api/src/Propely.PropertiesApi.Domain/Properties/Events/PropertyDeletedV1.cs`
- `services/properties-api/src/Propely.PropertiesApi.Domain/Properties/Exceptions/PropertyValidationException.cs`
- `services/properties-api/tests/Propely.PropertiesApi.UnitTests/Properties/PropertyTests.cs`
- `services/properties-api/tests/Propely.PropertiesApi.UnitTests/Properties/PropertyStatusTransitionTests.cs`
- `services/properties-api/tests/Propely.PropertiesApi.UnitTests/Properties/ValueObjects/AddressTests.cs`
- `services/properties-api/tests/Propely.PropertiesApi.UnitTests/Properties/ValueObjects/LocalizedTextTests.cs`

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `Property.Create()` with valid data raises `PropertyCreatedV1` | xUnit + FluentAssertions |
| Unit | `Property.Create()` with missing title throws | xUnit |
| Unit | All valid status transitions succeed | xUnit (parameterized) |
| Unit | All invalid status transitions throw `DomainException` | xUnit (parameterized) |
| Unit | `PricePerSqm` calculation with valid/zero/null values | xUnit |
| Unit | `Address` latitude/longitude validation | xUnit |
| Unit | `LocalizedText` requires at least one non-empty value | xUnit |
| Unit | `Property.ChangeStatus(Active)` sets `PublishedAt` on first activation | xUnit |
| Unit | `PropertyFeatures` validation (non-negative values) | xUnit |

### Security Considerations

- Domain entities have no external dependencies (pure domain model)
- Validation at domain level prevents invalid state regardless of caller
- `TenantId` is always required to enforce tenant isolation at the domain level

### TDD Reminder

Write all unit tests first. Start with the status state machine tests (all valid and invalid transitions), then value object validation tests, then factory method tests. Once all tests are red, implement the domain entities to make them green.

---

## Task 2.2 -- Property Persistence & Repository

**Status:** PENDING
**Dependencies:** 2.1 (domain model must exist)

### Goal

Implement EF Core persistence for the Property aggregate, including entity configuration, migrations, repository interfaces, and seed data. This establishes the data access layer for all property operations.

### Scope

**In scope:**
- EF Core entity configurations for `Property` and its value objects (owned types)
- `IPropertyRepository` interface in Application layer
- `IPropertyReadRepository` interface (read-optimized queries, no tracking)
- `PropertyRepository` implementation in Infrastructure layer
- Initial EF Core migration creating the `Properties` table
- Seed data: sample properties for development
- Database indexes: TenantId, AgentId, Status, PropertyType, OperationType

**Out of scope:**
- API endpoints (task 2.3)
- Full-text search (task 2.10)
- Media storage (task 2.5)

### Acceptance Criteria

- [ ] **AC1:** `PropertyConfiguration` maps `Property` entity with all value objects as owned types
- [ ] **AC2:** `Address` is stored as owned entity with columns prefixed `Address_`
- [ ] **AC3:** `PropertyFeatures` is stored as owned entity with columns prefixed `Features_`
- [ ] **AC4:** `PropertyFinancials` is stored as owned entity with columns prefixed `Financials_`
- [ ] **AC5:** `LocalizedText` is stored as owned entity with columns for `_Es`, `_Pt`, `_En`
- [ ] **AC6:** Database has indexes on `TenantId`, `AgentId`, `Status`, `PropertyType`, composite index on `(TenantId, Status)`
- [ ] **AC7:** `IPropertyRepository` includes: `AddAsync`, `UpdateAsync`, `DeleteAsync`, `GetByIdAsync(id, tenantId)`
- [ ] **AC8:** `IPropertyReadRepository` includes: `ListAsync(tenantId, filters, pagination, sorting)`, `GetByIdAsync(id, tenantId)`, `CountByStatusAsync(tenantId)`
- [ ] **AC9:** Initial migration creates `Properties` table with all columns
- [ ] **AC10:** Seed data includes at least 5 sample properties with varied types, operations, and statuses
- [ ] **AC11:** Soft-delete filter is applied globally via `HasQueryFilter`
- [ ] **AC12:** Integration tests verify CRUD operations against Testcontainers PostgreSQL

### Implementation Steps

1. Create `PropertiesApiDbContext` in Infrastructure layer
2. Create `PropertyConfiguration` (IEntityTypeConfiguration) with owned type mappings
3. Create `IPropertyRepository` in Application/Properties/Interfaces/
4. Create `IPropertyReadRepository` in Application/Properties/Interfaces/
5. Create `PropertyRepository` implementation in Infrastructure/Persistence/
6. Register DbContext and repositories in `DependencyInjection.cs`
7. Generate initial EF Core migration
8. Create seed data class with sample properties
9. Add database indexes
10. Write integration tests with Testcontainers

### Files to Create/Modify

**Create:**
- `services/properties-api/src/Propely.PropertiesApi.Infrastructure/Persistence/PropertiesApiDbContext.cs`
- `services/properties-api/src/Propely.PropertiesApi.Infrastructure/Persistence/Configurations/PropertyConfiguration.cs`
- `services/properties-api/src/Propely.PropertiesApi.Infrastructure/Persistence/Repositories/PropertyRepository.cs`
- `services/properties-api/src/Propely.PropertiesApi.Infrastructure/Persistence/Migrations/` (auto-generated)
- `services/properties-api/src/Propely.PropertiesApi.Infrastructure/Persistence/Seed/PropertySeeder.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Interfaces/IPropertyRepository.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Interfaces/IPropertyReadRepository.cs`
- `services/properties-api/tests/Propely.PropertiesApi.Infrastructure.Tests/Persistence/PropertyRepositoryTests.cs`

**Modify:**
- `services/properties-api/src/Propely.PropertiesApi.Infrastructure/DependencyInjection.cs`

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Integration | Create property, retrieve by ID, verify all fields | Testcontainers + xUnit |
| Integration | List properties by tenant, verify tenant isolation | Testcontainers + xUnit |
| Integration | Update property, verify owned types updated correctly | Testcontainers + xUnit |
| Integration | Soft-delete, verify query filter excludes deleted | Testcontainers + xUnit |
| Integration | Count by status returns correct counts | Testcontainers + xUnit |
| Integration | Pagination and sorting work correctly | Testcontainers + xUnit |

### Security Considerations

- All repository methods require `tenantId` parameter to enforce tenant isolation
- Soft-delete filter prevents accidental exposure of deleted data
- No raw SQL queries -- all access through EF Core to prevent SQL injection

### TDD Reminder

Write integration tests first defining expected behavior for all repository methods. Use Testcontainers for a real PostgreSQL instance. Run tests (they fail), then implement the repository and configurations to make them pass.

---

## Task 2.3 -- Property CRUD API

**Status:** PENDING
**Dependencies:** 2.2 (persistence layer must exist)

### Goal

Expose RESTful API endpoints for creating, reading, updating, deleting, and changing status of properties. Implement MediatR commands, queries, DTOs, and validators following the existing CQRS pattern.

### Scope

**In scope:**
- Commands: `CreateProperty`, `UpdateProperty`, `DeleteProperty`, `ChangePropertyStatus`
- Queries: `GetPropertyById`, `ListProperties` (with filters, pagination, sorting)
- DTOs: `PropertyDto`, `PropertyListItemDto`, `CreatePropertyRequest`, `UpdatePropertyRequest`, `ChangeStatusRequest`
- FluentValidation validators for all commands
- `PropertiesController` with standard REST endpoints
- Tenant context from JWT claims (`X-Tenant-Id` header)
- Pagination response envelope with `items`, `totalCount`, `page`, `pageSize`

**Out of scope:**
- Media upload (task 2.5)
- JSON Forms schema (task 2.4)
- Full-text search (task 2.10)
- SDK client (task 2.6)

### Acceptance Criteria

- [ ] **AC1:** `POST /api/properties` creates a property and returns 201 with `PropertyDto`
- [ ] **AC2:** `GET /api/properties/{id}` returns 200 with `PropertyDto` or 404
- [ ] **AC3:** `GET /api/properties` returns paginated list with `PropertyListItemDto[]`, supports `page`, `pageSize`, `sortBy`, `sortOrder` query params
- [ ] **AC4:** `PUT /api/properties/{id}` updates a property and returns 200 with updated `PropertyDto`
- [ ] **AC5:** `DELETE /api/properties/{id}` soft-deletes and returns 204
- [ ] **AC6:** `PATCH /api/properties/{id}/status` changes status and returns 200; returns 400 for invalid transitions
- [ ] **AC7:** All endpoints require authentication (401 if missing JWT)
- [ ] **AC8:** All endpoints enforce tenant isolation (properties only visible within tenant)
- [ ] **AC9:** `CreatePropertyRequest` validation: title required (max 200 chars), price >= 0, valid enum values
- [ ] **AC10:** `ListProperties` supports filter params: `type`, `operation`, `status`, `minPrice`, `maxPrice`, `city`, `agentId`
- [ ] **AC11:** API returns proper error responses with problem details (RFC 7807)
- [ ] **AC12:** All endpoints have integration tests

### Implementation Steps

1. Create DTOs in `Application/Properties/Dtos/`
2. Create `CreatePropertyCommand` + handler + validator
3. Create `UpdatePropertyCommand` + handler + validator
4. Create `DeletePropertyCommand` + handler
5. Create `ChangePropertyStatusCommand` + handler + validator
6. Create `GetPropertyByIdQuery` + handler
7. Create `ListPropertiesQuery` + handler (with filter/sort/pagination)
8. Create `PropertiesController` with all endpoints
9. Register MediatR in `Program.cs`
10. Write integration tests for all endpoints

### Files to Create/Modify

**Create:**
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Dtos/PropertyDto.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Dtos/PropertyListItemDto.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Dtos/CreatePropertyRequest.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Dtos/UpdatePropertyRequest.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Dtos/ChangeStatusRequest.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Commands/CreateProperty/CreatePropertyCommand.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Commands/CreateProperty/CreatePropertyCommandHandler.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Commands/CreateProperty/CreatePropertyCommandValidator.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Commands/UpdateProperty/UpdatePropertyCommand.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Commands/UpdateProperty/UpdatePropertyCommandHandler.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Commands/UpdateProperty/UpdatePropertyCommandValidator.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Commands/DeleteProperty/DeletePropertyCommand.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Commands/DeleteProperty/DeletePropertyCommandHandler.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Commands/ChangeStatus/ChangePropertyStatusCommand.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Commands/ChangeStatus/ChangePropertyStatusCommandHandler.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Commands/ChangeStatus/ChangePropertyStatusCommandValidator.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Queries/GetPropertyById/GetPropertyByIdQuery.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Queries/GetPropertyById/GetPropertyByIdQueryHandler.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Queries/ListProperties/ListPropertiesQuery.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Queries/ListProperties/ListPropertiesQueryHandler.cs`
- `services/properties-api/src/Propely.PropertiesApi.Api/Controllers/PropertiesController.cs`
- `services/properties-api/tests/Propely.PropertiesApi.UnitTests/Properties/Commands/CreatePropertyCommandHandlerTests.cs`
- `services/properties-api/tests/Propely.PropertiesApi.UnitTests/Properties/Commands/CreatePropertyCommandValidatorTests.cs`
- `services/properties-api/tests/Propely.PropertiesApi.IntegrationTests/Controllers/PropertiesControllerTests.cs`

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | Command handlers create/update/delete correctly | xUnit, mock repository |
| Unit | Validators reject invalid input (missing title, negative price, invalid enum) | xUnit |
| Unit | Query handlers return correct DTOs with mapping | xUnit, mock repository |
| Integration | Full CRUD cycle via HTTP (create, get, update, delete) | WebApplicationFactory + Testcontainers |
| Integration | Pagination returns correct pages and total count | WebApplicationFactory |
| Integration | Filter by type, operation, status, price range | WebApplicationFactory |
| Integration | Tenant isolation: tenant A cannot see tenant B's properties | WebApplicationFactory |
| Integration | Invalid status change returns 400 with problem details | WebApplicationFactory |

### Security Considerations

- All endpoints require valid JWT token
- Tenant isolation enforced at repository level via `tenantId` from claims
- Input validation prevents oversized payloads (max title 200 chars, max description 10000 chars)
- Price validation prevents negative values

### TDD Reminder

Write integration tests first for each endpoint. Define expected status codes, response shapes, and error cases. Then implement commands, queries, and controller to make tests pass. Write unit tests for validators separately.

---

## Task 2.4 -- Property JSON Schema & Validation

**Status:** PENDING
**Dependencies:** 2.1 (domain model defines the property structure)

### Goal

Define a JSON Schema for the property data structure and integrate `@jsonforms/react` with `@jsonforms/material-renderers` in the frontend for dynamic, schema-driven property data entry. Create custom renderers for property-specific fields.

### Scope

**In scope:**
- JSON Schema (draft 2020-12) defining the complete property structure
- Schema versioning mechanism (schema version stored with each property)
- Custom JSON Forms renderers for: energy rating selector, orientation picker, property type selector with icons, operation type selector
- Integration of `@jsonforms/react` and `@jsonforms/material-renderers` in the frontend
- UI schema defining form layout (field ordering, grouping, conditional visibility)
- Validation messages in Spanish and English

**Out of scope:**
- Full form wizard UI (task 2.8)
- API endpoints (task 2.3)
- Media fields (handled separately in task 2.5)

### Acceptance Criteria

- [ ] **AC1:** JSON Schema defines all property fields with correct types, enums, and constraints
- [ ] **AC2:** Schema includes `$schema` version identifier for future migrations
- [ ] **AC3:** Custom `EnergyRatingRenderer` shows A-G scale with color coding (A=green, G=red)
- [ ] **AC4:** Custom `OrientationRenderer` shows compass rose selector
- [ ] **AC5:** Custom `PropertyTypeRenderer` shows type options with descriptive icons
- [ ] **AC6:** UI Schema groups fields into logical sections: Basic Info, Location, Features, Financial, Description
- [ ] **AC7:** Conditional visibility: `plotArea` only shows for `Land`, `House`, `Villa`; `floor` only for `Apartment`, `Penthouse`, `Studio`
- [ ] **AC8:** Validation messages display in the user's locale (ES/EN)
- [ ] **AC9:** JSON Forms `onChange` callback returns validated data matching the `CreatePropertyRequest` DTO shape
- [ ] **AC10:** Custom renderers have unit tests verifying render and interaction behavior

### Implementation Steps

1. Define JSON Schema file for property data structure
2. Define UI Schema file for form layout
3. Install `@jsonforms/react` and `@jsonforms/material-renderers`
4. Create custom renderer: `EnergyRatingRenderer`
5. Create custom renderer: `OrientationRenderer`
6. Create custom renderer: `PropertyTypeRenderer`
7. Create custom renderer: `OperationTypeRenderer`
8. Register all custom renderers with JSON Forms
9. Create `PropertyJsonForm` wrapper component that combines schema + UI schema + renderers
10. Write unit tests for custom renderers
11. Write integration test for full form rendering and data output

### Files to Create/Modify

**Create:**
- `apps/web/src/schemas/property-schema.json`
- `apps/web/src/schemas/property-ui-schema.json`
- `apps/web/src/components/properties/json-forms/EnergyRatingRenderer.tsx`
- `apps/web/src/components/properties/json-forms/OrientationRenderer.tsx`
- `apps/web/src/components/properties/json-forms/PropertyTypeRenderer.tsx`
- `apps/web/src/components/properties/json-forms/OperationTypeRenderer.tsx`
- `apps/web/src/components/properties/json-forms/renderers.ts` (registry)
- `apps/web/src/components/properties/json-forms/PropertyJsonForm.tsx`
- `apps/web/src/components/properties/json-forms/__tests__/EnergyRatingRenderer.test.tsx`
- `apps/web/src/components/properties/json-forms/__tests__/OrientationRenderer.test.tsx`
- `apps/web/src/components/properties/json-forms/__tests__/PropertyTypeRenderer.test.tsx`
- `apps/web/src/components/properties/json-forms/__tests__/PropertyJsonForm.test.tsx`

**Modify:**
- `apps/web/package.json` (add `@jsonforms/react`, `@jsonforms/core`, `@jsonforms/material-renderers`)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `EnergyRatingRenderer` renders all ratings with correct colors | Vitest + RTL |
| Unit | `OrientationRenderer` renders compass directions, handles selection | Vitest + RTL |
| Unit | `PropertyTypeRenderer` renders all types with icons | Vitest + RTL |
| Unit | `PropertyJsonForm` renders complete form from schema | Vitest + RTL |
| Unit | Conditional visibility: `plotArea` hidden for `Apartment` | Vitest + RTL |
| Unit | Validation messages appear for required fields | Vitest + RTL |
| Unit | `onChange` callback returns properly shaped data | Vitest + RTL |

### Security Considerations

- JSON Schema validation happens client-side for UX and server-side for security
- Schema files are static assets; no user-modifiable schemas
- Input sanitization for free-text fields (title, description) to prevent XSS

### TDD Reminder

Write renderer tests first defining expected visual output for each property type, energy rating, and orientation. Then implement renderers to pass. Test the full form by providing sample data and verifying onChange output matches expected DTO shape.

---

## Task 2.5 -- Property Media Management

**Status:** PENDING
**Dependencies:** 2.2 (property persistence must exist for linking media to properties)

### Goal

Enable agents to upload, manage, and serve property media (photos and floor plans) via GCP Cloud Storage. Implement upload, resize/optimize, ordering, and deletion with signed URL generation for secure access.

### Scope

**In scope:**
- `PropertyMedia` entity: id, propertyId, type (Photo, FloorPlan), storagePath, fileName, contentType, sizeBytes, width, height, displayOrder, uploadedAt
- GCP Cloud Storage integration via `Google.Cloud.Storage.V1`
- Photo upload: accept JPEG/PNG/WebP, max 10 MB per file, max 30 photos per property
- Automatic image optimization: resize to max 2048px width, generate thumbnail (400px width)
- Floor plan upload: accept JPEG/PNG/PDF, max 20 MB
- Signed URL generation (1-hour expiry) for secure media access
- Reorder photos (update `displayOrder`)
- Delete individual media files (removes from storage and database)
- `POST /api/properties/{id}/media` (multipart upload)
- `DELETE /api/properties/{id}/media/{mediaId}`
- `PATCH /api/properties/{id}/media/reorder` (batch update order)
- `GET /api/properties/{id}/media` (list with signed URLs)

**Out of scope:**
- Frontend upload UI (part of task 2.8 and 2.9)
- Video upload/processing
- Virtual tour hosting

### Acceptance Criteria

- [ ] **AC1:** `PropertyMedia` entity tracks file metadata: path, size, dimensions, content type, display order
- [ ] **AC2:** `POST /api/properties/{id}/media` accepts multipart upload, stores in GCP, returns `PropertyMediaDto`
- [ ] **AC3:** Photos are resized to max 2048px width and a 400px thumbnail is generated on upload
- [ ] **AC4:** Upload validates: file type (JPEG/PNG/WebP for photos), max size (10 MB photo, 20 MB floor plan), max count (30 photos)
- [ ] **AC5:** `GET /api/properties/{id}/media` returns media list with signed URLs (1-hour expiry)
- [ ] **AC6:** `DELETE /api/properties/{id}/media/{mediaId}` removes file from GCP and database record
- [ ] **AC7:** `PATCH /api/properties/{id}/media/reorder` accepts array of `{mediaId, displayOrder}` and updates order
- [ ] **AC8:** GCP bucket is organized by tenant: `{tenantId}/properties/{propertyId}/{fileName}`
- [ ] **AC9:** Media operations enforce tenant isolation (cannot access other tenant's media)
- [ ] **AC10:** Integration tests use a mock storage provider for unit tests and real GCP emulator for integration tests

### Implementation Steps

1. Create `PropertyMedia` entity in Domain layer
2. Create `MediaType` enum: `Photo`, `FloorPlan`
3. Create `IStorageService` interface in Application layer
4. Create `GcpStorageService` implementation in Infrastructure layer
5. Create `IImageProcessingService` interface and `ImageProcessingService` (SkiaSharp or ImageSharp)
6. Create upload command, handler, validator
7. Create delete command and handler
8. Create reorder command and handler
9. Create list query and handler (with signed URL generation)
10. Create `PropertyMediaController` with endpoints
11. Write tests with mock storage

### Files to Create/Modify

**Create:**
- `services/properties-api/src/Propely.PropertiesApi.Domain/Properties/PropertyMedia.cs`
- `services/properties-api/src/Propely.PropertiesApi.Domain/Properties/MediaType.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Interfaces/IStorageService.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Interfaces/IImageProcessingService.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Dtos/PropertyMediaDto.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Commands/UploadMedia/UploadMediaCommand.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Commands/UploadMedia/UploadMediaCommandHandler.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Commands/DeleteMedia/DeleteMediaCommand.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Commands/DeleteMedia/DeleteMediaCommandHandler.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Commands/ReorderMedia/ReorderMediaCommand.cs`
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Commands/ReorderMedia/ReorderMediaCommandHandler.cs`
- `services/properties-api/src/Propely.PropertiesApi.Infrastructure/Storage/GcpStorageService.cs`
- `services/properties-api/src/Propely.PropertiesApi.Infrastructure/Storage/ImageProcessingService.cs`
- `services/properties-api/src/Propely.PropertiesApi.Api/Controllers/PropertyMediaController.cs`
- `services/properties-api/tests/Propely.PropertiesApi.UnitTests/Properties/Commands/UploadMediaCommandHandlerTests.cs`
- `services/properties-api/tests/Propely.PropertiesApi.UnitTests/Properties/Commands/DeleteMediaCommandHandlerTests.cs`

**Modify:**
- `services/properties-api/src/Propely.PropertiesApi.Infrastructure/Persistence/Configurations/PropertyMediaConfiguration.cs` (new)
- `services/properties-api/src/Propely.PropertiesApi.Infrastructure/DependencyInjection.cs`

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | Upload handler validates file type, size, count limits | xUnit, mock services |
| Unit | Upload handler calls storage service and image processing | xUnit, mock services |
| Unit | Delete handler removes from storage and database | xUnit, mock services |
| Unit | Reorder handler updates display order correctly | xUnit, mock repository |
| Integration | Full upload cycle: upload file, verify stored, retrieve with signed URL | WebApplicationFactory, mock GCP |
| Integration | Delete removes file from storage | WebApplicationFactory, mock GCP |

### Security Considerations

- File type validation by content inspection (magic bytes), not just extension
- Signed URLs prevent unauthorized direct access to stored media
- Upload size limits prevent resource exhaustion
- Tenant-scoped storage paths prevent cross-tenant access
- No executable file types accepted

### TDD Reminder

Write upload validation tests first (file type, size, count limits). Then write storage integration tests with mock. Finally implement the services and handlers.

---

## Task 2.6 -- Properties-API NuGet SDK Client

**Status:** PENDING
**Dependencies:** 2.3 (CRUD API must exist), 0.4 (NuGet SDK client infrastructure from P0)

### Goal

Create a Refit-based NuGet SDK client package (`Propely.PropertiesApi.Client`) that other services can consume to query property data. Includes DTOs, typed client interface, tenant header propagation, and Polly resilience policies.

### Scope

**In scope:**
- `IPropertiesApiClient` interface with Refit attributes for all property endpoints
- Shared DTOs matching API response shapes
- `TenantDelegatingHandler` for `X-Tenant-Id` header propagation
- Polly v8 retry (3 retries, exponential backoff) and circuit breaker (5 failures, 30s break) policies
- `ServiceCollectionExtensions.AddPropertiesApiClient(url)` registration method
- Package metadata (version, description, dependencies)

**Out of scope:**
- Media upload via SDK (direct API call needed for multipart)
- Write operations (SDK is read-only for cross-service use)

### Acceptance Criteria

- [ ] **AC1:** `IPropertiesApiClient` interface exposes: `GetByIdAsync(id)`, `ListAsync(filters)`, `CountByStatusAsync(tenantId)`
- [ ] **AC2:** Client DTOs match API response shapes exactly
- [ ] **AC3:** `TenantDelegatingHandler` propagates `X-Tenant-Id` from `IHttpContextAccessor` or explicit parameter
- [ ] **AC4:** Polly retry policy retries on transient HTTP errors (5xx, 408, 429) with exponential backoff
- [ ] **AC5:** Polly circuit breaker opens after 5 consecutive failures, half-opens after 30 seconds
- [ ] **AC6:** `AddPropertiesApiClient(url)` registers `IPropertiesApiClient` in DI with all policies
- [ ] **AC7:** SDK version follows SemVer and matches the service API version
- [ ] **AC8:** Unit tests verify retry and circuit breaker behavior with mock HTTP handlers
- [ ] **AC9:** Integration test verifies SDK client can call running properties-api and get valid responses
- [ ] **AC10:** Package has XML documentation for all public types

### Implementation Steps

1. Create `Propely.PropertiesApi.Client` project in `services/properties-api/src/`
2. Define `IPropertiesApiClient` with Refit attributes
3. Create shared DTO classes (read-only records)
4. Create `TenantDelegatingHandler`
5. Create `ServiceCollectionExtensions` with Polly policies
6. Add package metadata to `.csproj`
7. Write unit tests for handler and policies
8. Write integration tests

### Files to Create/Modify

**Create:**
- `services/properties-api/src/Propely.PropertiesApi.Client/Propely.PropertiesApi.Client.csproj`
- `services/properties-api/src/Propely.PropertiesApi.Client/IPropertiesApiClient.cs`
- `services/properties-api/src/Propely.PropertiesApi.Client/Dtos/PropertyDto.cs`
- `services/properties-api/src/Propely.PropertiesApi.Client/Dtos/PropertyListItemDto.cs`
- `services/properties-api/src/Propely.PropertiesApi.Client/Dtos/PropertyStatusCount.cs`
- `services/properties-api/src/Propely.PropertiesApi.Client/Http/TenantDelegatingHandler.cs`
- `services/properties-api/src/Propely.PropertiesApi.Client/ServiceCollectionExtensions.cs`
- `services/properties-api/tests/Propely.PropertiesApi.UnitTests/TenantDelegatingHandlerTests.cs`
- `services/properties-api/tests/Propely.PropertiesApi.UnitTests/ResiliencePolicyTests.cs`

**Modify:**
- `services/properties-api/Propely.PropertiesApi.sln` (add Client project)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `TenantDelegatingHandler` adds `X-Tenant-Id` header | xUnit, mock `IHttpContextAccessor` |
| Unit | Retry policy retries on 500, 408, 429 | xUnit, mock `HttpMessageHandler` |
| Unit | Circuit breaker opens after 5 failures | xUnit, mock `HttpMessageHandler` |
| Integration | SDK client calls running API and returns valid data | WebApplicationFactory |

### Security Considerations

- SDK client uses service-to-service authentication (not user tokens)
- `X-Tenant-Id` propagation ensures tenant isolation across service boundaries
- No sensitive data in SDK package (no connection strings or secrets)

### TDD Reminder

Write handler tests first verifying header propagation. Write resilience tests simulating transient failures and verifying retry/circuit breaker behavior. Then implement the client.

---

## Task 2.7 -- Property List UI

**Status:** PENDING
**Dependencies:** 2.3 (API endpoints must exist for data fetching)

### Goal

Build the property listing page with table/card views, filters, pagination, sorting, and quick actions. This is the primary property management interface for agents.

### Scope

**In scope:**
- Use case definition for property list screen
- Stitch MCP design generation (Project ID: 16786124142182555397, model: GEMINI_3_PRO)
- Table view with columns: thumbnail, title, type, operation, price, status, city, agent, updated date
- Card view (grid) with property cards showing thumbnail, title, price, key features
- View toggle (table/card)
- Filter bar: type, operation, status, price range, city
- Pagination controls (page selector, page size selector)
- Sort by: price, date listed, area, title
- Status badges with color coding (Draft=gray, Active=green, Reserved=yellow, Sold=blue, Rented=purple, Archived=red)
- Quick actions: change status, edit, delete (with confirmation)
- Empty state for no properties
- Responsive design

**Out of scope:**
- Full-text search (task 2.10)
- Advanced filters panel (task 2.10)
- Property creation (task 2.8)

### Acceptance Criteria

- [ ] **AC1:** Property list page renders a table with all columns and data from API
- [ ] **AC2:** Card view shows property cards in a responsive grid (1-col mobile, 2-col tablet, 3-col desktop)
- [ ] **AC3:** View toggle switches between table and card views, persisted in localStorage
- [ ] **AC4:** Filter bar supports: type (dropdown), operation (dropdown), status (multi-select), price range (min/max inputs), city (text input)
- [ ] **AC5:** Filters update URL query params (shareable filter state)
- [ ] **AC6:** Pagination controls show page numbers, previous/next, and page size selector (10/25/50)
- [ ] **AC7:** Sort dropdown allows sorting by price, date, area, title in asc/desc order
- [ ] **AC8:** Status badges show correct colors for each status
- [ ] **AC9:** Quick actions: "Change Status" opens status transition dropdown, "Edit" navigates to edit form, "Delete" shows confirmation dialog
- [ ] **AC10:** Empty state shows illustration and "Create your first property" CTA button
- [ ] **AC11:** All components have unit tests; Stitch design exists and implementation matches pixel-for-pixel

### Implementation Steps

1. Define use cases for property list interactions
2. Generate Stitch MCP designs for: table view, card view, filter bar, empty state
3. Download Stitch HTML to `.stitch-html/property-list-table.html`, `.stitch-html/property-list-cards.html`
4. Create `useProperties` data fetching hook with filter/sort/pagination state
5. Create `PropertyTable` component
6. Create `PropertyCard` component
7. Create `PropertyFilterBar` component
8. Create `PropertyStatusBadge` component
9. Create `PropertyQuickActions` component
10. Create `ViewToggle` component
11. Create `PropertyListPage` composing all components
12. Add route: `/properties`
13. Write component tests
14. Verify against Stitch designs

### Files to Create/Modify

**Create:**
- `apps/web/src/hooks/useProperties.ts`
- `apps/web/src/components/properties/PropertyTable.tsx`
- `apps/web/src/components/properties/PropertyCard.tsx`
- `apps/web/src/components/properties/PropertyFilterBar.tsx`
- `apps/web/src/components/properties/PropertyStatusBadge.tsx`
- `apps/web/src/components/properties/PropertyQuickActions.tsx`
- `apps/web/src/components/properties/ViewToggle.tsx`
- `apps/web/src/app/[locale]/(dashboard)/properties/page.tsx`
- `apps/web/src/components/properties/__tests__/PropertyTable.test.tsx`
- `apps/web/src/components/properties/__tests__/PropertyCard.test.tsx`
- `apps/web/src/components/properties/__tests__/PropertyFilterBar.test.tsx`
- `apps/web/src/components/properties/__tests__/PropertyStatusBadge.test.tsx`
- `.stitch-html/property-list-table.html`
- `.stitch-html/property-list-cards.html`

**Modify:**
- `apps/web/src/messages/en.json` (add property translation keys)
- `apps/web/src/messages/es.json` (add property translation keys)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `PropertyTable` renders rows with correct data | Vitest + RTL |
| Unit | `PropertyCard` renders thumbnail, title, price, features | Vitest + RTL |
| Unit | `PropertyFilterBar` updates filter state on change | Vitest + RTL |
| Unit | `PropertyStatusBadge` renders correct color per status | Vitest + RTL |
| Unit | `ViewToggle` switches views and persists to localStorage | Vitest + RTL |
| Unit | Empty state renders when no properties | Vitest + RTL |
| Unit | Pagination updates page and refetches | Vitest + RTL, mock fetch |
| Manual | Visual comparison against Stitch designs | Dev environment |

### Security Considerations

- Property data only shows properties for the current tenant
- Delete confirmation prevents accidental data loss
- No sensitive data (e.g., agent contact info) exposed in list view

### TDD Reminder

Write component tests first defining expected renders for: table with 5 properties, card grid, each status badge color, empty state, filter interactions. Then implement components to pass tests. Verify against Stitch designs last.

---

## Task 2.8 -- Property Create/Edit Form

**Status:** PENDING
**Dependencies:** 2.3 (CRUD API), 2.4 (JSON Forms schema and renderers)

### Goal

Build a multi-step form wizard for creating and editing properties using JSON Forms. The wizard guides agents through a logical progression of property data entry with draft saving at each step.

### Scope

**In scope:**
- Use case definition for property create/edit flow
- Stitch MCP design generation for each form step
- Multi-step wizard with 6 steps:
  - Step 1: Basic Info (type, operation, title)
  - Step 2: Location (address with map preview if coordinates provided)
  - Step 3: Features (bedrooms, bathrooms, areas, amenities)
  - Step 4: Financial (price, community fees, IBI)
  - Step 5: Descriptions (multilingual tabs: ES, PT, EN)
  - Step 6: Media upload (photos, floor plans)
- Draft save: auto-save to localStorage every 30 seconds; explicit "Save as Draft" button
- Submit: validates all steps, sends `POST /api/properties` or `PUT /api/properties/{id}`
- Edit mode: pre-fills form with existing property data
- Step navigation: linear progression with ability to go back; step indicators show completion
- Validation: per-step validation before allowing next step

**Out of scope:**
- AI Smart-Fill integration (Phase 3)
- Map picker for location (show read-only map preview with coordinates if available)

### Acceptance Criteria

- [ ] **AC1:** Create form navigates through 6 steps in order with visual step indicator
- [ ] **AC2:** Each step validates its fields before allowing navigation to next step
- [ ] **AC3:** Steps 1-5 use JSON Forms with the schema from Task 2.4
- [ ] **AC4:** Step 6 uses drag-and-drop photo upload with preview thumbnails
- [ ] **AC5:** "Save as Draft" button saves current state to API with status `Draft`
- [ ] **AC6:** Auto-save to localStorage every 30 seconds prevents data loss
- [ ] **AC7:** "Publish" button validates all steps and creates property with status `Active`
- [ ] **AC8:** Edit mode pre-fills all steps with existing property data
- [ ] **AC9:** Step 5 (Descriptions) shows language tabs (ES, PT, EN) with rich text or textarea
- [ ] **AC10:** Responsive design: steps stack vertically on mobile
- [ ] **AC11:** All components have unit tests; Stitch designs exist for each step

### Implementation Steps

1. Define use cases for create and edit flows
2. Generate Stitch MCP designs for: step indicator, each of the 6 steps, review summary
3. Download Stitch HTML files
4. Create `PropertyFormWizard` container component managing step state
5. Create `StepIndicator` component
6. Create step components: `BasicInfoStep`, `LocationStep`, `FeaturesStep`, `FinancialStep`, `DescriptionsStep`, `MediaStep`
7. Create `usePropertyForm` hook managing form state, validation, draft saving
8. Create `useAutoSave` hook for localStorage persistence
9. Integrate JSON Forms in steps 1-5
10. Create drag-and-drop media upload component for step 6
11. Add routes: `/properties/new`, `/properties/[id]/edit`
12. Write component tests
13. Verify against Stitch designs

### Files to Create/Modify

**Create:**
- `apps/web/src/components/properties/form/PropertyFormWizard.tsx`
- `apps/web/src/components/properties/form/StepIndicator.tsx`
- `apps/web/src/components/properties/form/steps/BasicInfoStep.tsx`
- `apps/web/src/components/properties/form/steps/LocationStep.tsx`
- `apps/web/src/components/properties/form/steps/FeaturesStep.tsx`
- `apps/web/src/components/properties/form/steps/FinancialStep.tsx`
- `apps/web/src/components/properties/form/steps/DescriptionsStep.tsx`
- `apps/web/src/components/properties/form/steps/MediaStep.tsx`
- `apps/web/src/hooks/usePropertyForm.ts`
- `apps/web/src/hooks/useAutoSave.ts`
- `apps/web/src/app/[locale]/(dashboard)/properties/new/page.tsx`
- `apps/web/src/app/[locale]/(dashboard)/properties/[id]/edit/page.tsx`
- `apps/web/src/components/properties/form/__tests__/PropertyFormWizard.test.tsx`
- `apps/web/src/components/properties/form/__tests__/StepIndicator.test.tsx`
- `apps/web/src/components/properties/form/__tests__/BasicInfoStep.test.tsx`
- `.stitch-html/property-form-wizard.html`
- `.stitch-html/property-form-basic.html`
- `.stitch-html/property-form-location.html`
- `.stitch-html/property-form-features.html`

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `PropertyFormWizard` navigates between steps | Vitest + RTL |
| Unit | `StepIndicator` highlights current step, shows completed steps | Vitest + RTL |
| Unit | `BasicInfoStep` validates required fields before next | Vitest + RTL |
| Unit | `MediaStep` handles drag-and-drop and shows previews | Vitest + RTL |
| Unit | `useAutoSave` saves to localStorage on interval | Vitest |
| Unit | `usePropertyForm` manages state across steps | Vitest |
| Unit | Edit mode pre-fills all steps correctly | Vitest + RTL, mock API |
| Manual | Full create flow end-to-end | Dev environment |

### Security Considerations

- File upload validation (type, size) on both client and server
- Draft data in localStorage is cleared after successful submission
- No sensitive data stored in localStorage drafts

### TDD Reminder

Write tests for step navigation (forward, backward, validation blocking), auto-save behavior, and edit mode pre-fill. Then implement components to pass. Test media upload separately.

---

## Task 2.9 -- Property Detail View

**Status:** PENDING
**Dependencies:** 2.3 (API to fetch property data), 2.5 (media management for gallery)

### Goal

Build the property detail page showing all property information, a photo gallery with lightbox, status actions, and navigation. This is the primary property viewing interface.

### Scope

**In scope:**
- Use case definition for property detail view
- Stitch MCP design generation
- Hero section with primary photo and photo gallery
- Photo gallery with lightbox (click to enlarge, navigate between photos)
- Property info sections: Basic Info, Location (with map if coordinates), Features, Financial, Descriptions
- Status action bar: contextual buttons based on current status (e.g., "Activate" for Draft, "Reserve" for Active)
- Status change confirmation dialog
- "Edit" button navigating to edit form
- "Back to List" navigation
- Floor plan viewer
- Breadcrumb navigation

**Out of scope:**
- Publication status (Phase 6)
- Appointment scheduling from property view (Phase 5)
- Contact/lead linking (Phase 4)

### Acceptance Criteria

- [ ] **AC1:** Property detail page renders all property data organized in sections
- [ ] **AC2:** Photo gallery shows thumbnails with lightbox on click; supports keyboard navigation (arrow keys, Escape)
- [ ] **AC3:** Status action bar shows only valid transitions as buttons (e.g., Draft shows "Activate" and "Archive")
- [ ] **AC4:** Clicking a status action shows confirmation dialog: "Change status from [current] to [new]?"
- [ ] **AC5:** Successful status change updates the UI immediately without page reload
- [ ] **AC6:** Location section shows embedded map preview when latitude/longitude are available
- [ ] **AC7:** Financial section shows formatted price (with currency symbol) and calculated price per sqm
- [ ] **AC8:** Descriptions section shows multilingual tabs (ES, PT, EN) with only available languages
- [ ] **AC9:** Edit button navigates to `/properties/{id}/edit`
- [ ] **AC10:** Responsive design: sections stack vertically on mobile, gallery adapts to screen width
- [ ] **AC11:** All components have unit tests; Stitch design exists and implementation matches

### Implementation Steps

1. Define use cases for property detail interactions
2. Generate Stitch MCP designs for: detail page, photo gallery, lightbox, status actions
3. Download Stitch HTML files
4. Create `useProperty` data fetching hook (single property by ID)
5. Create `PropertyDetailPage` container
6. Create `PropertyPhotoGallery` component with lightbox
7. Create `PropertyInfoSections` component (Basic, Location, Features, Financial, Descriptions)
8. Create `PropertyStatusActions` component with transition buttons
9. Create `StatusChangeDialog` confirmation component
10. Create `PropertyMap` component (static map preview)
11. Add route: `/properties/[id]`
12. Write component tests
13. Verify against Stitch designs

### Files to Create/Modify

**Create:**
- `apps/web/src/hooks/useProperty.ts`
- `apps/web/src/components/properties/detail/PropertyDetailPage.tsx`
- `apps/web/src/components/properties/detail/PropertyPhotoGallery.tsx`
- `apps/web/src/components/properties/detail/PropertyInfoSections.tsx`
- `apps/web/src/components/properties/detail/PropertyStatusActions.tsx`
- `apps/web/src/components/properties/detail/StatusChangeDialog.tsx`
- `apps/web/src/components/properties/detail/PropertyMap.tsx`
- `apps/web/src/components/properties/detail/PropertyFloorPlans.tsx`
- `apps/web/src/app/[locale]/(dashboard)/properties/[id]/page.tsx`
- `apps/web/src/components/properties/detail/__tests__/PropertyDetailPage.test.tsx`
- `apps/web/src/components/properties/detail/__tests__/PropertyPhotoGallery.test.tsx`
- `apps/web/src/components/properties/detail/__tests__/PropertyStatusActions.test.tsx`
- `.stitch-html/property-detail.html`
- `.stitch-html/property-gallery-lightbox.html`

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `PropertyDetailPage` renders all sections with correct data | Vitest + RTL, mock fetch |
| Unit | `PropertyPhotoGallery` renders thumbnails, opens lightbox on click | Vitest + RTL |
| Unit | `PropertyStatusActions` renders only valid transitions per status | Vitest + RTL (parameterized per status) |
| Unit | `StatusChangeDialog` shows current/new status, calls API on confirm | Vitest + RTL, mock fetch |
| Unit | `PropertyInfoSections` formats price with currency symbol | Vitest + RTL |
| Unit | Descriptions tabs show only available languages | Vitest + RTL |
| Manual | Visual comparison against Stitch designs | Dev environment |

### Security Considerations

- Property detail only accessible within the same tenant
- Status changes require appropriate permissions (checked via API)
- Map embed does not leak precise location to unauthorized users

### TDD Reminder

Write tests defining expected renders for: a fully populated property (all fields), a minimal property (only required fields), each status with its valid action buttons. Then implement components.

---

## Task 2.10 -- Property Search & Advanced Filters

**Status:** PENDING
**Dependencies:** 2.3 (API must support filter queries)

### Goal

Implement full-text search and advanced filtering for properties with URL-based filter state for shareable searches and optional saved filter presets.

### Scope

**In scope:**
- Full-text search on: title (all languages), description (all languages), address (city, province, street)
- Backend: PostgreSQL `tsvector` / `tsquery` or `ILIKE` based search with ranking
- Advanced filter panel (expandable/collapsible):
  - Property type (multi-select)
  - Operation type (multi-select)
  - Status (multi-select)
  - Price range (min-max slider + inputs)
  - Area range (min-max for builtArea)
  - Bedrooms (min)
  - Bathrooms (min)
  - City (text autocomplete)
  - Province (dropdown)
  - Agent (dropdown, admin/owner only)
  - Features checkboxes (hasPool, hasGarden, hasGarage, hasElevator, hasTerrace)
- URL-based filter state: all filters serialized to query params
- Sort options: relevance (when searching), price asc/desc, date listed, area, title
- Saved filter presets: save current filter combination with a name, recall later
- Search debounce (300ms)

**Out of scope:**
- Elasticsearch or external search engine (use PostgreSQL native search)
- Geospatial search (nearby properties)

### Acceptance Criteria

- [ ] **AC1:** Search input in header performs full-text search with 300ms debounce
- [ ] **AC2:** Search results are ranked by relevance when a search term is active
- [ ] **AC3:** Advanced filter panel expands/collapses with smooth animation
- [ ] **AC4:** All filter values are serialized to URL query params and survive page reload
- [ ] **AC5:** Copying the URL with filters and opening in new tab reproduces the same filter state
- [ ] **AC6:** "Clear all filters" button resets all filters and search
- [ ] **AC7:** Active filter count is shown as a badge on the filter toggle button
- [ ] **AC8:** Backend `GET /api/properties` supports `search` query param triggering full-text search
- [ ] **AC9:** Backend full-text search uses PostgreSQL `tsvector` with Spanish, Portuguese, and English configurations
- [ ] **AC10:** Saved presets are stored per-user in localStorage (with option to persist to API later)
- [ ] **AC11:** Feature checkboxes filter properties that have the matching boolean flags
- [ ] **AC12:** All components have unit tests

### Implementation Steps

1. Add full-text search column and index to `Properties` table (GIN index on `tsvector`)
2. Create migration adding `SearchVector` computed column
3. Update `ListPropertiesQuery` to support `search` parameter with `tsquery`
4. Add multi-language text search configuration (ES, PT, EN)
5. Update `PropertyFilterBar` to include search input
6. Create `AdvancedFilterPanel` component
7. Create `FilterPresetManager` component (save/load presets)
8. Create `usePropertyFilters` hook managing all filter state with URL serialization
9. Create `ActiveFilterBadges` component showing active filters as removable chips
10. Write backend integration tests for search
11. Write frontend component tests

### Files to Create/Modify

**Create:**
- `apps/web/src/components/properties/search/AdvancedFilterPanel.tsx`
- `apps/web/src/components/properties/search/FilterPresetManager.tsx`
- `apps/web/src/components/properties/search/ActiveFilterBadges.tsx`
- `apps/web/src/hooks/usePropertyFilters.ts`
- `apps/web/src/components/properties/search/__tests__/AdvancedFilterPanel.test.tsx`
- `apps/web/src/components/properties/search/__tests__/FilterPresetManager.test.tsx`
- `apps/web/src/components/properties/search/__tests__/usePropertyFilters.test.ts`

**Modify:**
- `services/properties-api/src/Propely.PropertiesApi.Infrastructure/Persistence/Configurations/PropertyConfiguration.cs` (add search vector)
- `services/properties-api/src/Propely.PropertiesApi.Infrastructure/Persistence/Migrations/` (new migration)
- `services/properties-api/src/Propely.PropertiesApi.Application/Properties/Queries/ListProperties/ListPropertiesQueryHandler.cs` (add search support)
- `apps/web/src/components/properties/PropertyFilterBar.tsx` (add search input)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Integration | Full-text search returns relevant results for Spanish text | Testcontainers + xUnit |
| Integration | Full-text search across title + description + address | Testcontainers + xUnit |
| Integration | Combined filters (type + price range + search) return correct results | Testcontainers + xUnit |
| Unit | `AdvancedFilterPanel` renders all filter controls | Vitest + RTL |
| Unit | `usePropertyFilters` serializes/deserializes URL params correctly | Vitest |
| Unit | Search input debounces with 300ms delay | Vitest |
| Unit | `FilterPresetManager` saves/loads presets from localStorage | Vitest |
| Unit | `ActiveFilterBadges` shows active filters and removes on click | Vitest + RTL |

### Security Considerations

- Search input is sanitized to prevent SQL injection (parameterized queries through EF Core)
- URL params are validated on parse (invalid values ignored, not throwing errors)
- Filter presets in localStorage are scoped per user (by userId key prefix)

### TDD Reminder

Write backend integration tests for full-text search first (Spanish property descriptions, multi-field search, ranking). Then write frontend tests for filter state management and URL serialization. Implement to pass tests.

---

## Summary

| Task | Title | Status | Dependencies |
|---|---|---|---|
| 2.1 | Property Domain Model | PENDING | 0.3 |
| 2.2 | Property Persistence & Repository | PENDING | 2.1 |
| 2.3 | Property CRUD API | PENDING | 2.2 |
| 2.4 | Property JSON Schema & Validation | PENDING | 2.1 |
| 2.5 | Property Media Management | PENDING | 2.2 |
| 2.6 | Properties-API NuGet SDK Client | PENDING | 2.3, 0.4 |
| 2.7 | Property List UI | PENDING | 2.3 |
| 2.8 | Property Create/Edit Form | PENDING | 2.3, 2.4 |
| 2.9 | Property Detail View | PENDING | 2.3, 2.5 |
| 2.10 | Property Search & Advanced Filters | PENDING | 2.3 |

## Dependency Graph

```
Phase 0 (prerequisites)
  0.3 Scaffold ──────── 0.4 NuGet SDK Infrastructure
  Properties-API            │
       │                    │
       ▼                    │
  2.1 Property              │
  Domain Model              │
   ├─────────┬──────┐       │
   ▼         ▼      ▼       │
  2.2       2.4    (2.1)    │
  Persistence JSON          │
  & Repository Schema       │
   ├────┬────┐              │
   ▼    ▼    ▼              │
  2.3  2.5   │              │
  CRUD Media │              │
  API  Mgmt  │              │
   │    │    │              │
   ├────┼────┼──────────────┘
   │    │    │              │
   ▼    │    ▼              ▼
  2.6   │   2.8           2.6
  SDK   │   Create/Edit   SDK Client
  Client│   Form (2.3+2.4)
   │    │
   │    ▼
   │   2.9 Detail View (2.3+2.5)
   │
   ├───► 2.7 Property List UI
   │
   └───► 2.10 Search & Filters
```

## Completion Criteria

Phase 2 is complete when:
1. `Property` aggregate root exists with complete domain model (types, operations, statuses, value objects)
2. Status state machine enforces valid transitions with domain events
3. EF Core persistence stores properties with owned value objects and proper indexes
4. RESTful CRUD API exposes all property operations with tenant isolation
5. JSON Schema defines the property data structure with custom JSON Forms renderers
6. GCP Cloud Storage integration handles photo/floor plan upload, resize, and signed URL access
7. `Propely.PropertiesApi.Client` SDK enables other services to query property data
8. Property list page supports table/card views, filters, pagination, and sorting
9. Multi-step create/edit form wizard with draft save and JSON Forms integration
10. Property detail page with photo gallery, status actions, and responsive layout
11. Full-text search works across title, description, and address in ES/PT/EN
12. All features have unit tests, integration tests, and Stitch designs
