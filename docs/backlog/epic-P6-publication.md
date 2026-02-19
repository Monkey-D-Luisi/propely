# Epic P6 -- Portal Publication

## Overview

Enable real estate agents to publish property listings to external portals (Kyero, Thribee/Trovit/Mitula/Nestoria/Nuroa, SpainHouses) and receive leads back from those portals. The system generates portal-specific XML feeds, validates against each portal's XSD schema, and publishes through a background worker. Inbound lead webhooks from portals are parsed, deduplicated, and routed to the contacts/leads system. Each portal has its own adapter implementing a common interface, making it straightforward to add new portals in the future.

## Service Ownership

| Capability | Service |
|---|---|
| Publishing domain model | `services/publishing-api` (Domain layer) |
| Portal adapters (XML generation) | `services/publishing-api` (Infrastructure layer) |
| Publishing worker | `services/publishing-api` (hosted service) |
| Lead intake webhooks | `services/publishing-api` (API layer) |
| Cross-service lead creation | `services/publishing-api` via Contacts-API SDK |
| Frontend publication management | `apps/web` |

## Tasks

| # | Title | Status | Dependencies |
|---|---|---|---|
| 6.1 | Publishing Domain Model | PENDING | -- |
| 6.2 | Portal Adapter: Kyero | PENDING | 6.1 |
| 6.3 | Portal Adapter: Thribee | PENDING | 6.1 |
| 6.4 | Portal Adapter: SpainHouses | PENDING | 6.1 |
| 6.5 | Publishing Worker | PENDING | 6.1, 6.2, 6.3, 6.4 |
| 6.6 | Lead Intake Webhooks | PENDING | 6.1, P4 (4.5) |
| 6.7 | Frontend Publication Management | PENDING | 6.5 |

---

## Task 6.1 -- Publishing Domain Model

**Status:** PENDING
**Dependencies:** None

### Scope

**In scope:**
- `Publication` aggregate root entity in `PublishingApi.Domain`
- Fields: `PropertyId`, `Portal` (enum), `Status`, `FeedUrl`, `ExternalListingId`, `LastPublishedUtc`, `LastError`
- `Portal` enum: `Kyero`, `Thribee`, `SpainHouses` (extensible for future portals)
- `PublicationStatus` enum: `Draft`, `Pending`, `Published`, `Failed`, `Unpublished`
- `IPortalAdapter` interface: defines the contract all portal adapters must implement
- Domain events: `PublicationCreatedV1`, `PublicationPublishedV1`, `PublicationFailedV1`, `PublicationUnpublishedV1`
- Business rules: one publication per property per portal (unique constraint), cannot re-publish while status is Pending
- `PublicationAttempt` value object tracking individual publish attempts (timestamp, success/error, response)
- Soft-delete support

**Out of scope:**
- Specific portal adapter implementations (tasks 6.2--6.4)
- Background worker (task 6.5)
- Lead intake (task 6.6)

### Acceptance Criteria

- [ ] `Publication` entity inherits from `Entity` and implements `ISoftDeletable`
- [ ] `Publication` has required fields: `PropertyId`, `Portal`, `TenantId`
- [ ] `Publication` has managed fields: `Status`, `FeedUrl`, `ExternalListingId`, `LastPublishedUtc`, `LastError`
- [ ] `Portal` enum includes: `Kyero`, `Thribee`, `SpainHouses`
- [ ] `PublicationStatus` enum includes: `Draft`, `Pending`, `Published`, `Failed`, `Unpublished`
- [ ] `Publication.Create()` sets status to `Draft`, raises `PublicationCreatedV1`
- [ ] `Publication.RequestPublish()` sets status to `Pending` (only from `Draft` or `Failed` or `Unpublished`)
- [ ] `Publication.MarkPublished(feedUrl, externalId)` sets status to `Published`, stores URL and external ID, raises `PublicationPublishedV1`
- [ ] `Publication.MarkFailed(error)` sets status to `Failed`, stores error, raises `PublicationFailedV1`
- [ ] `Publication.Unpublish()` sets status to `Unpublished`, raises `PublicationUnpublishedV1`
- [ ] Cannot `RequestPublish()` while status is `Pending` (throws `DomainException`)
- [ ] `Publication.AddAttempt(PublicationAttempt)` tracks publish history
- [ ] `PublicationAttempt` value object: `AttemptedAtUtc`, `Success` (bool), `ResponseCode`, `ErrorMessage`
- [ ] `IPortalAdapter` interface: `GenerateXmlAsync(Property, Publication)`, `ValidateXml(string xml)`, `GetPortal()` returning `Portal` enum
- [ ] One publication per property per portal enforced at domain level (static validation method)
- [ ] All domain events include `PublicationId`, `PropertyId`, `Portal`, `TenantId`, `OccurredAt`
- [ ] Unit tests cover all status transitions, validation rules, and domain events

### Implementation Steps

1. Create `Portal` enum in `Domain/Publications/`
2. Create `PublicationStatus` enum in `Domain/Publications/`
3. Create `PublicationAttempt` value object in `Domain/Publications/`
4. Create `Publication` aggregate root with factory methods and status transitions
5. Create domain events in `Domain/Publications/Events/`
6. Create `IPortalAdapter` interface in `Domain/Publications/` (domain interface, implemented in Infrastructure)
7. Create `PublicationValidationException` in `Domain/Publications/Exceptions/`
8. Write comprehensive unit tests

### Files to Create/Modify

**Create:**
- `services/publishing-api/src/Propely.PublishingApi.Domain/Publications/Publication.cs`
- `services/publishing-api/src/Propely.PublishingApi.Domain/Publications/Portal.cs`
- `services/publishing-api/src/Propely.PublishingApi.Domain/Publications/PublicationStatus.cs`
- `services/publishing-api/src/Propely.PublishingApi.Domain/Publications/PublicationAttempt.cs`
- `services/publishing-api/src/Propely.PublishingApi.Domain/Publications/IPortalAdapter.cs`
- `services/publishing-api/src/Propely.PublishingApi.Domain/Publications/Events/PublicationCreatedV1.cs`
- `services/publishing-api/src/Propely.PublishingApi.Domain/Publications/Events/PublicationPublishedV1.cs`
- `services/publishing-api/src/Propely.PublishingApi.Domain/Publications/Events/PublicationFailedV1.cs`
- `services/publishing-api/src/Propely.PublishingApi.Domain/Publications/Events/PublicationUnpublishedV1.cs`
- `services/publishing-api/src/Propely.PublishingApi.Domain/Publications/Exceptions/PublicationValidationException.cs`
- `services/publishing-api/tests/Propely.PublishingApi.Domain.Tests/Publications/PublicationTests.cs`
- `services/publishing-api/tests/Propely.PublishingApi.Domain.Tests/Publications/PublicationStatusTransitionTests.cs`

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `Publication.Create()` sets status Draft, raises event | xUnit + FluentAssertions |
| Unit | `Publication.RequestPublish()` from valid states succeeds | xUnit, parameterized |
| Unit | `Publication.RequestPublish()` from Pending throws | xUnit |
| Unit | `Publication.MarkPublished()` stores feedUrl and externalId | xUnit |
| Unit | `Publication.MarkFailed()` stores error message | xUnit |
| Unit | `Publication.Unpublish()` from Published succeeds | xUnit |
| Unit | All valid/invalid status transitions (parameterized matrix) | xUnit `[Theory]` |
| Unit | `PublicationAttempt` stores correct data | xUnit |
| Unit | Duplicate property+portal detection | xUnit |
| Unit | All domain events contain correct IDs | xUnit |

### Security & Privacy

- Publication metadata (feed URLs, external IDs) is not PII but is tenant-scoped
- Error messages from portals may contain API keys in error responses; sanitize before storing
- `IPortalAdapter` implementations will handle credentials; interface itself is clean

### TDD Reminder

Write status transition matrix tests first (parameterized for all combinations). Write factory method tests. Then implement the aggregate.

---

## Task 6.2 -- Portal Adapter: Kyero

**Status:** PENDING
**Dependencies:** 6.1 (publishing domain model and `IPortalAdapter` interface)

### Scope

**In scope:**
- `KyeroPortalAdapter` implementing `IPortalAdapter`
- Generate Kyero XML v3.9 format from property data
- Map Propely property fields to Kyero XML schema elements
- XSD validation against Kyero's published schema
- Handle Kyero-specific property type mappings (apartment, villa, townhouse, finca, country house, etc.)
- Currency, area units, and energy certificate mapping
- Image URL inclusion in feed
- Multi-language descriptions (es, en at minimum)

**Out of scope:**
- Other portal adapters (tasks 6.3, 6.4)
- Feed hosting/serving (task 6.5)
- Direct API upload to Kyero (Kyero uses feed URL pull model)

### Acceptance Criteria

- [ ] `KyeroPortalAdapter` implements `IPortalAdapter`
- [ ] `GenerateXmlAsync()` produces valid Kyero XML v3.9
- [ ] XML includes root `<kyero>` element with feed metadata (version, language)
- [ ] Each property mapped to a `<property>` element with all required Kyero fields
- [ ] Required Kyero fields mapped: `id`, `date`, `ref`, `price`, `currency`, `type`, `town`, `province`, `country`, `location_detail`, `beds`, `baths`, `surface_area`
- [ ] Optional fields mapped when available: `pool`, `parking`, `garden`, `url`, `desc` (multi-language), `images`, `features`, `energy_rating`
- [ ] Propely property types mapped to Kyero types: Apartment->Apartment, Villa->Villa, Townhouse->Townhouse, etc.
- [ ] `desc` element supports `en` and `es` sub-elements
- [ ] `images` element contains ordered `<image>` elements with `<url>` children
- [ ] `ValidateXml()` validates generated XML against Kyero XSD v3.9
- [ ] Validation returns list of errors (empty list = valid)
- [ ] XSD schema file embedded as project resource
- [ ] Generated XML is UTF-8 encoded with XML declaration
- [ ] Unit tests validate XML structure for various property configurations
- [ ] Unit tests confirm XSD validation catches invalid XML

### Implementation Steps

1. Download and embed Kyero v3.9 XSD schema as project resource
2. Create `KyeroPropertyTypeMapper` for property type enum mapping
3. Create `KyeroXmlBuilder` utility for constructing the XML document
4. Create `KyeroPortalAdapter` implementing `IPortalAdapter`
5. Implement `GenerateXmlAsync` with full field mapping
6. Implement `ValidateXml` using `XmlSchemaSet` validation
7. Handle edge cases: missing optional fields, null images, empty descriptions
8. Write unit tests with sample property data generating expected XML
9. Write XSD validation tests (valid and invalid XML)

### Files to Create/Modify

**Create:**
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Portals/Kyero/KyeroPortalAdapter.cs`
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Portals/Kyero/KyeroXmlBuilder.cs`
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Portals/Kyero/KyeroPropertyTypeMapper.cs`
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Portals/Kyero/Schemas/kyero-v3.9.xsd` (embedded resource)
- `services/publishing-api/tests/Propely.PublishingApi.Infrastructure.Tests/Portals/Kyero/KyeroPortalAdapterTests.cs`
- `services/publishing-api/tests/Propely.PublishingApi.Infrastructure.Tests/Portals/Kyero/KyeroXmlBuilderTests.cs`
- `services/publishing-api/tests/Propely.PublishingApi.Infrastructure.Tests/Portals/Kyero/KyeroPropertyTypeMapperTests.cs`
- `services/publishing-api/tests/Propely.PublishingApi.Infrastructure.Tests/Portals/Kyero/TestData/` (sample property data)

**Modify:**
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/DependencyInjection.cs` (register Kyero adapter)
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Propely.PublishingApi.Infrastructure.csproj` (embed XSD resource)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `KyeroPropertyTypeMapper` maps all Propely types to Kyero types | xUnit, parameterized |
| Unit | `KyeroXmlBuilder` generates correct XML structure | xUnit, `XDocument` assertions |
| Unit | Full property generates valid Kyero XML | xUnit, validate against XSD |
| Unit | Minimal property (only required fields) generates valid XML | xUnit, validate against XSD |
| Unit | Missing optional fields do not break XML generation | xUnit |
| Unit | Multi-language descriptions render correctly | xUnit, XPath assertions |
| Unit | Images are ordered correctly in XML | xUnit |
| Unit | `ValidateXml` catches missing required elements | xUnit, intentionally invalid XML |
| Unit | `ValidateXml` passes for valid XML | xUnit |
| Manual | Generate Kyero XML for real property, verify in Kyero validator | Dev environment |

### Security & Privacy

- Generated XML may contain property addresses (not PII but business-sensitive)
- Feed URLs should be protected with a unique, unguessable token per tenant
- No agent/owner personal data in the XML feed

### TDD Reminder

Write XSD validation tests first with known valid and invalid XML samples. Then write XML builder tests for each field mapping. Implement the adapter to pass all tests.

---

## Task 6.3 -- Portal Adapter: Thribee

**Status:** PENDING
**Dependencies:** 6.1 (publishing domain model and `IPortalAdapter` interface)

### Scope

**In scope:**
- `ThribeePortalAdapter` implementing `IPortalAdapter`
- Generate Thribee XML format compatible with Trovit, Mitula, Nestoria, and Nuroa
- Map Propely property fields to Thribee XML schema
- Thribee-specific field mappings: property type, transaction type (sale/rent), region codes
- XSD validation (if Thribee provides one; otherwise DTD or structural validation)
- Multi-language support
- Image URLs in feed
- Handle Thribee's specific requirements for each sub-portal

**Out of scope:**
- Other portal adapters (tasks 6.2, 6.4)
- Direct API integration (Thribee uses feed pull model)
- Individual sub-portal customization beyond the unified Thribee format

### Acceptance Criteria

- [ ] `ThribeePortalAdapter` implements `IPortalAdapter`
- [ ] `GenerateXmlAsync()` produces valid Thribee XML
- [ ] XML root element and namespace match Thribee specification
- [ ] Each property mapped to a Thribee listing element with required fields
- [ ] Required fields: `id`, `url`, `title`, `type` (sale/rent), `property_type`, `price`, `currency`, `city`, `region`, `latitude`, `longitude`
- [ ] Optional fields: `description`, `bedrooms`, `bathrooms`, `floor_area`, `plot_area`, `parking`, `pool`, `year_built`, `energy_certificate`
- [ ] Images included as `<picture>` elements with `<picture_url>` and `<picture_title>`
- [ ] Transaction type correctly mapped: sale properties -> `For Sale`, rental properties -> `For Rent`
- [ ] Propely property types mapped to Thribee categories
- [ ] `ValidateXml()` validates structural correctness
- [ ] Generated XML is UTF-8 encoded
- [ ] Unit tests cover XML generation for sale and rental properties
- [ ] Unit tests cover all property type mappings

### Implementation Steps

1. Research and document Thribee XML feed specification
2. Create `ThribeePropertyTypeMapper` for type mapping
3. Create `ThribeeTransactionTypeMapper` for sale/rent mapping
4. Create `ThribeeXmlBuilder` utility
5. Create `ThribeePortalAdapter` implementing `IPortalAdapter`
6. Implement `GenerateXmlAsync` with field mapping
7. Implement `ValidateXml` with structural validation
8. Handle geo-coordinates (latitude/longitude) requirement
9. Write unit tests

### Files to Create/Modify

**Create:**
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Portals/Thribee/ThribeePortalAdapter.cs`
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Portals/Thribee/ThribeeXmlBuilder.cs`
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Portals/Thribee/ThribeePropertyTypeMapper.cs`
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Portals/Thribee/ThribeeTransactionTypeMapper.cs`
- `services/publishing-api/tests/Propely.PublishingApi.Infrastructure.Tests/Portals/Thribee/ThribeePortalAdapterTests.cs`
- `services/publishing-api/tests/Propely.PublishingApi.Infrastructure.Tests/Portals/Thribee/ThribeeXmlBuilderTests.cs`
- `services/publishing-api/tests/Propely.PublishingApi.Infrastructure.Tests/Portals/Thribee/ThribeePropertyTypeMapperTests.cs`
- `services/publishing-api/tests/Propely.PublishingApi.Infrastructure.Tests/Portals/Thribee/TestData/` (sample data)

**Modify:**
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/DependencyInjection.cs` (register Thribee adapter)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | Property type mapper covers all Propely types | xUnit, parameterized |
| Unit | Transaction type mapper: sale vs. rent | xUnit |
| Unit | XML builder generates correct structure for sale property | xUnit, XDocument assertions |
| Unit | XML builder generates correct structure for rental property | xUnit |
| Unit | Geo-coordinates rendered correctly | xUnit |
| Unit | Images ordered with titles | xUnit |
| Unit | `ValidateXml` catches structural issues | xUnit |
| Unit | Minimal property generates valid XML | xUnit |
| Manual | Submit generated XML to Thribee feed validator | Dev environment |

### Security & Privacy

- Same considerations as Kyero adapter (task 6.2)
- Geo-coordinates may reveal exact property location; included intentionally for portal display
- No personal data in feed

### TDD Reminder

Write XML structure tests with expected output for sale and rental properties first. Then implement the builder and adapter.

---

## Task 6.4 -- Portal Adapter: SpainHouses

**Status:** PENDING
**Dependencies:** 6.1 (publishing domain model and `IPortalAdapter` interface)

### Scope

**In scope:**
- `SpainHousesPortalAdapter` implementing `IPortalAdapter`
- Generate SpainHouses XML format for Spain domestic market
- Map Propely property fields to SpainHouses schema
- XSD validation against SpainHouses' published schema
- Spain-specific fields: catastro reference, IBI tax, community fees, energy certificate class
- Province and municipality codes (INE codes for Spain)
- Multi-language descriptions with emphasis on Spanish

**Out of scope:**
- Other portal adapters (tasks 6.2, 6.3)
- International property support (SpainHouses is Spain-only)

### Acceptance Criteria

- [ ] `SpainHousesPortalAdapter` implements `IPortalAdapter`
- [ ] `GenerateXmlAsync()` produces valid SpainHouses XML
- [ ] XML matches SpainHouses schema specification
- [ ] Required fields mapped: `id`, `reference`, `operation` (sale/rent), `type`, `price`, `province_code`, `municipality_code`, `address`
- [ ] Optional fields mapped: `catastro_reference`, `ibi_tax`, `community_fees`, `energy_certificate_class`, `energy_certificate_value`
- [ ] Spain-specific property types: piso, atico, bajo, duplex, adosado, chalet, finca, cortijo, local, oficina, nave, solar, garaje
- [ ] Province codes mapped to INE two-digit codes (e.g., Malaga = 29, Barcelona = 08)
- [ ] Municipality codes mapped to INE five-digit codes
- [ ] Images included with order index
- [ ] `ValidateXml()` validates against SpainHouses XSD
- [ ] XSD schema file embedded as project resource
- [ ] Properties outside Spain are rejected with a validation error
- [ ] Unit tests cover Spain-specific field mappings
- [ ] Unit tests confirm XSD validation

### Implementation Steps

1. Obtain and embed SpainHouses XSD schema as project resource
2. Create `SpainHousesPropertyTypeMapper` with Spanish property type terminology
3. Create `SpainHousesProvinceMapper` for INE province codes
4. Create `SpainHousesMunicipalityMapper` for INE municipality codes
5. Create `SpainHousesXmlBuilder` utility
6. Create `SpainHousesPortalAdapter` implementing `IPortalAdapter`
7. Implement `GenerateXmlAsync` with Spain-specific field handling
8. Implement `ValidateXml` using `XmlSchemaSet`
9. Handle catastro reference, IBI tax, community fees, energy certificate
10. Write unit tests with focus on Spain-specific mappings

### Files to Create/Modify

**Create:**
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Portals/SpainHouses/SpainHousesPortalAdapter.cs`
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Portals/SpainHouses/SpainHousesXmlBuilder.cs`
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Portals/SpainHouses/SpainHousesPropertyTypeMapper.cs`
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Portals/SpainHouses/SpainHousesProvinceMapper.cs`
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Portals/SpainHouses/SpainHousesMunicipalityMapper.cs`
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Portals/SpainHouses/Schemas/spainhouses.xsd` (embedded resource)
- `services/publishing-api/tests/Propely.PublishingApi.Infrastructure.Tests/Portals/SpainHouses/SpainHousesPortalAdapterTests.cs`
- `services/publishing-api/tests/Propely.PublishingApi.Infrastructure.Tests/Portals/SpainHouses/SpainHousesXmlBuilderTests.cs`
- `services/publishing-api/tests/Propely.PublishingApi.Infrastructure.Tests/Portals/SpainHouses/SpainHousesPropertyTypeMapperTests.cs`
- `services/publishing-api/tests/Propely.PublishingApi.Infrastructure.Tests/Portals/SpainHouses/SpainHousesProvinceMapperTests.cs`
- `services/publishing-api/tests/Propely.PublishingApi.Infrastructure.Tests/Portals/SpainHouses/TestData/` (sample data)

**Modify:**
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/DependencyInjection.cs` (register SpainHouses adapter)
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Propely.PublishingApi.Infrastructure.csproj` (embed XSD resource)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | Spanish property type mapper covers all types (piso, atico, etc.) | xUnit, parameterized |
| Unit | Province code mapper returns correct INE codes | xUnit, parameterized for all 52 provinces |
| Unit | Municipality code mapper returns correct codes for common cities | xUnit, parameterized |
| Unit | XML builder generates valid SpainHouses XML | xUnit, XSD validation |
| Unit | Catastro reference, IBI tax, community fees rendered correctly | xUnit |
| Unit | Energy certificate class and value mapped | xUnit |
| Unit | Non-Spain property rejected | xUnit |
| Unit | `ValidateXml` catches schema violations | xUnit |
| Manual | Submit generated XML to SpainHouses for validation | Dev environment |

### Security & Privacy

- Catastro references are public registry data, not PII
- IBI tax amounts are property-specific, not owner-specific
- No personal data in feed XML
- Same feed URL protection as other portals

### TDD Reminder

Write province and municipality code mapping tests first (comprehensive, parameterized). Then property type mapping tests. Then XML generation and validation tests. Implement adapters last.

---

## Task 6.5 -- Publishing Worker

**Status:** PENDING
**Dependencies:** 6.1 (domain model), 6.2, 6.3, 6.4 (all portal adapters)

### Scope

**In scope:**
- `PublishingWorker` background hosted service
- Listen for `PropertyStatusChangedV1` domain events (published/unpublished/updated)
- When property is published: generate XML via appropriate `IPortalAdapter`, validate, store/serve feed
- When property is updated: regenerate XML feed
- When property is unpublished: remove from feed
- Feed serving endpoint: `GET /api/feeds/{tenantId}/{portal}` returns the XML feed URL for portal crawlers
- Retry failed publications with exponential backoff (max 5 retries)
- Track publication status per property per portal
- Dead-letter for permanently failed publications
- Telemetry: publications per portal, success/failure rates, generation duration

**Out of scope:**
- Lead intake (task 6.6)
- Frontend (task 6.7)
- Push-based publication (all portals use feed pull model initially)
- Individual property XML endpoints (single feed per tenant per portal)

### Acceptance Criteria

- [ ] `PublishingWorker` hosted service starts with the application
- [ ] On `PropertyStatusChangedV1` (status -> Active): creates Publication records for all enabled portals, sets status to Pending
- [ ] Worker picks up Pending publications and generates XML via the correct `IPortalAdapter`
- [ ] Generated XML is validated against portal schema before being marked Published
- [ ] If validation fails: mark Failed with validation error details
- [ ] XML feed stored in a location accessible via `GET /api/feeds/{tenantId}/{portal}`
- [ ] Feed endpoint returns `Content-Type: application/xml` with UTF-8 encoding
- [ ] Feed includes ALL published properties for the tenant for that portal (aggregated feed)
- [ ] When a property is updated: feed is regenerated to include updated data
- [ ] When a property is unpublished: feed is regenerated without that property
- [ ] Retry: failed publications retried up to 5 times with exponential backoff (1s, 2s, 4s, 8s, 16s)
- [ ] After 5 failures: publication moves to dead-letter, admin notified
- [ ] `PublicationAttempt` records saved for each attempt (success or failure)
- [ ] Feed URL is unique per tenant per portal, includes a security token
- [ ] Worker processes publications in batches (configurable batch size, default 10)
- [ ] Telemetry counters: `publishing.generated`, `publishing.validated`, `publishing.failed`, `publishing.duration_ms`
- [ ] Unit tests for worker logic with mocked adapters
- [ ] Integration test for full publication flow

### Implementation Steps

1. Create `IPublicationRepository` interface in Application layer
2. Create `IPublicationReadRepository` interface
3. Create `IFeedStorageService` interface for storing/serving XML feeds
4. Create `PublishingWorker` as `BackgroundService`
5. Create MediatR event handler for `PropertyStatusChangedV1` that creates Publication records
6. Implement worker loop: fetch Pending publications, generate XML, validate, store
7. Create `LocalFeedStorageService` implementation (file-based for dev, blob storage for prod)
8. Create `FeedController` with `GET /api/feeds/{tenantId}/{portal}` endpoint
9. Implement feed security: unique token per tenant per portal in the URL
10. Implement retry logic with exponential backoff
11. Implement dead-letter mechanism
12. Add `PublicationConfiguration` EF Core configuration
13. Create `PublicationRepository` implementation
14. Add migration for publications and publication_attempts tables
15. Register services in DI
16. Add telemetry instrumentation
17. Write unit tests
18. Write integration test

### Files to Create/Modify

**Create:**
- `services/publishing-api/src/Propely.PublishingApi.Application/Publications/Interfaces/IPublicationRepository.cs`
- `services/publishing-api/src/Propely.PublishingApi.Application/Publications/Interfaces/IPublicationReadRepository.cs`
- `services/publishing-api/src/Propely.PublishingApi.Application/Publications/Interfaces/IFeedStorageService.cs`
- `services/publishing-api/src/Propely.PublishingApi.Application/Publications/Dtos/PublicationDto.cs`
- `services/publishing-api/src/Propely.PublishingApi.Application/Publications/Dtos/PublicationListItemDto.cs`
- `services/publishing-api/src/Propely.PublishingApi.Application/Publications/Dtos/FeedStatusDto.cs`
- `services/publishing-api/src/Propely.PublishingApi.Application/Publications/EventHandlers/PropertyStatusChangedPublicationHandler.cs`
- `services/publishing-api/src/Propely.PublishingApi.Application/Publications/Commands/RetryPublication/RetryPublicationCommand.cs`
- `services/publishing-api/src/Propely.PublishingApi.Application/Publications/Commands/RetryPublication/RetryPublicationCommandHandler.cs`
- `services/publishing-api/src/Propely.PublishingApi.Application/Publications/Commands/UnpublishProperty/UnpublishPropertyCommand.cs`
- `services/publishing-api/src/Propely.PublishingApi.Application/Publications/Commands/UnpublishProperty/UnpublishPropertyCommandHandler.cs`
- `services/publishing-api/src/Propely.PublishingApi.Application/Publications/Queries/ListPublications/ListPublicationsQuery.cs`
- `services/publishing-api/src/Propely.PublishingApi.Application/Publications/Queries/ListPublications/ListPublicationsQueryHandler.cs`
- `services/publishing-api/src/Propely.PublishingApi.Application/Publications/Queries/GetPublicationStatus/GetPublicationStatusQuery.cs`
- `services/publishing-api/src/Propely.PublishingApi.Application/Publications/Queries/GetPublicationStatus/GetPublicationStatusQueryHandler.cs`
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Publishing/PublishingWorker.cs`
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Publishing/LocalFeedStorageService.cs`
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Persistence/Configurations/PublicationConfiguration.cs`
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Persistence/Configurations/PublicationAttemptConfiguration.cs`
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Persistence/Repositories/PublicationRepository.cs`
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Persistence/Repositories/PublicationReadRepository.cs`
- `services/publishing-api/src/Propely.PublishingApi.Api/Controllers/FeedController.cs`
- `services/publishing-api/src/Propely.PublishingApi.Api/Controllers/PublicationsController.cs`
- `services/publishing-api/tests/Propely.PublishingApi.Infrastructure.Tests/Publishing/PublishingWorkerTests.cs`
- `services/publishing-api/tests/Propely.PublishingApi.Application.Tests/Publications/EventHandlers/PropertyStatusChangedPublicationHandlerTests.cs`
- `services/publishing-api/tests/Propely.PublishingApi.Api.Tests/Controllers/FeedControllerTests.cs`

**Modify:**
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Persistence/OrgsApiDbContext.cs` (add `DbSet<Publication>`)
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/DependencyInjection.cs` (register worker, storage, repos)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | Event handler creates Publication records for enabled portals | xUnit, mock repos |
| Unit | Worker picks up Pending publications and calls correct adapter | xUnit, mock adapters |
| Unit | Worker marks Published on success | xUnit |
| Unit | Worker marks Failed on validation error | xUnit |
| Unit | Retry logic: retries up to 5 times then dead-letters | xUnit, mock adapter throwing |
| Unit | Feed aggregation: all Published properties included | xUnit |
| Unit | Unpublished property removed from feed | xUnit |
| Integration | Full publication flow: event -> pending -> published -> feed served | `WebApplicationFactory` |
| Integration | Feed endpoint returns valid XML | `WebApplicationFactory` |
| Integration | Feed endpoint rejects invalid token | `WebApplicationFactory` |
| Manual | Publish property, verify XML feed content | Dev environment |

### Security & Privacy

- Feed URLs contain security tokens; tokens must be unguessable (UUID or HMAC-based)
- Feed endpoint does not require authentication (portals crawl anonymously) but requires valid token
- Rate limiting on feed endpoint to prevent scraping abuse
- Generated XML does not contain agent personal data
- Dead-lettered publications may contain error details; log without sensitive data

### TDD Reminder

Write event handler tests first (what publications are created). Then worker processing tests (success, failure, retry). Then feed serving tests. Implement each component to pass its tests.

---

## Task 6.6 -- Lead Intake Webhooks

**Status:** PENDING
**Dependencies:** 6.1 (domain model for Publication/portal identification), P4 task 4.5 (Contacts-API SDK client for lead creation)

### Scope

**In scope:**
- Per-portal webhook endpoints for receiving lead inquiries from portals
- `POST /api/webhooks/kyero/leads` -- parse Kyero lead format
- `POST /api/webhooks/thribee/leads` -- parse Thribee/Trovit lead format
- `POST /api/webhooks/spainhouses/leads` -- parse SpainHouses lead format
- Each webhook: parse incoming data, map to standard lead format, deduplicate by email + propertyId, create lead via Contacts-API SDK
- Lead is ALWAYS linked to the published property (matched by external listing ID or internal property reference)
- Webhook authentication: per-portal API keys or HMAC signatures
- Idempotency: same lead from same portal does not create duplicates
- Structured logging for all incoming webhook payloads (PII-safe)

**Out of scope:**
- Lead management UI (epic P4)
- Auto-assignment of leads to agents
- Lead notification emails (handled by contacts/leads service)
- Webhook retry from portal side (portals handle their own retry)

### Acceptance Criteria

- [ ] `POST /api/webhooks/kyero/leads` accepts Kyero lead format and creates a lead
- [ ] `POST /api/webhooks/thribee/leads` accepts Thribee lead format and creates a lead
- [ ] `POST /api/webhooks/spainhouses/leads` accepts SpainHouses lead format and creates a lead
- [ ] Each webhook parses portal-specific format into a common `IncomingLead` model
- [ ] `IncomingLead` model: `name`, `email`, `phone`, `message`, `portalPropertyId`, `portalName`
- [ ] Property matching: resolve `portalPropertyId` to internal `PropertyId` via `Publication.ExternalListingId`
- [ ] If property not found: log warning, return 200 (do not fail; portal may retry)
- [ ] Lead created via `ILeadsApi` (Contacts-API SDK client) with `Source` = portal name
- [ ] Deduplication: if lead with same email + propertyId exists, return 200 without creating duplicate
- [ ] Webhook authentication: `X-Api-Key` header validated per portal
- [ ] API keys stored in configuration (environment variables), not hardcoded
- [ ] Invalid/missing API key returns 401
- [ ] Malformed payload returns 400 with safe error message (no stack trace)
- [ ] All webhook requests logged with: portal, timestamp, property reference (NOT email/phone/name)
- [ ] Response is always 200 on successful processing (portals expect 2xx)
- [ ] Unit tests for each portal's payload parser
- [ ] Integration test for full webhook -> lead creation flow

### Implementation Steps

1. Create `IncomingLead` model in Application layer
2. Create `ILeadIntakeService` interface in Application layer
3. Create `IWebhookPayloadParser` interface with portal-specific implementations
4. Create `KyeroWebhookPayloadParser` parsing Kyero's lead notification format
5. Create `ThribeeWebhookPayloadParser` parsing Thribee's format
6. Create `SpainHousesWebhookPayloadParser` parsing SpainHouses' format
7. Create `LeadIntakeService` implementation: property resolution, deduplication, lead creation via SDK
8. Create `KyeroWebhookController` with API key validation
9. Create `ThribeeWebhookController` with API key validation
10. Create `SpainHousesWebhookController` with API key validation
11. Create `WebhookApiKeyAuthAttribute` or middleware for API key validation
12. Configure portal API keys in environment variables
13. Write unit tests for each parser
14. Write unit tests for `LeadIntakeService`
15. Write integration tests for webhook endpoints

### Files to Create/Modify

**Create:**
- `services/publishing-api/src/Propely.PublishingApi.Application/Publications/Models/IncomingLead.cs`
- `services/publishing-api/src/Propely.PublishingApi.Application/Publications/Interfaces/ILeadIntakeService.cs`
- `services/publishing-api/src/Propely.PublishingApi.Application/Publications/Interfaces/IWebhookPayloadParser.cs`
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Portals/Kyero/KyeroWebhookPayloadParser.cs`
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Portals/Thribee/ThribeeWebhookPayloadParser.cs`
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Portals/SpainHouses/SpainHousesWebhookPayloadParser.cs`
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/Publishing/LeadIntakeService.cs`
- `services/publishing-api/src/Propely.PublishingApi.Api/Controllers/Webhooks/KyeroWebhookController.cs`
- `services/publishing-api/src/Propely.PublishingApi.Api/Controllers/Webhooks/ThribeeWebhookController.cs`
- `services/publishing-api/src/Propely.PublishingApi.Api/Controllers/Webhooks/SpainHousesWebhookController.cs`
- `services/publishing-api/src/Propely.PublishingApi.Api/Middleware/WebhookApiKeyAuthAttribute.cs`
- `services/publishing-api/tests/Propely.PublishingApi.Infrastructure.Tests/Portals/Kyero/KyeroWebhookPayloadParserTests.cs`
- `services/publishing-api/tests/Propely.PublishingApi.Infrastructure.Tests/Portals/Thribee/ThribeeWebhookPayloadParserTests.cs`
- `services/publishing-api/tests/Propely.PublishingApi.Infrastructure.Tests/Portals/SpainHouses/SpainHousesWebhookPayloadParserTests.cs`
- `services/publishing-api/tests/Propely.PublishingApi.Infrastructure.Tests/Publishing/LeadIntakeServiceTests.cs`
- `services/publishing-api/tests/Propely.PublishingApi.Api.Tests/Controllers/Webhooks/KyeroWebhookControllerTests.cs`
- `services/publishing-api/tests/Propely.PublishingApi.Api.Tests/Controllers/Webhooks/ThribeeWebhookControllerTests.cs`
- `services/publishing-api/tests/Propely.PublishingApi.Api.Tests/Controllers/Webhooks/SpainHousesWebhookControllerTests.cs`

**Modify:**
- `services/publishing-api/src/Propely.PublishingApi.Infrastructure/DependencyInjection.cs` (register parsers, intake service)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `KyeroWebhookPayloadParser` parses valid Kyero lead payload | xUnit + FluentAssertions |
| Unit | `ThribeeWebhookPayloadParser` parses valid Thribee payload | xUnit |
| Unit | `SpainHousesWebhookPayloadParser` parses valid SpainHouses payload | xUnit |
| Unit | Each parser rejects malformed payloads gracefully | xUnit |
| Unit | `LeadIntakeService` resolves property from external listing ID | xUnit, mock publication repo |
| Unit | `LeadIntakeService` deduplicates by email + propertyId | xUnit, mock leads API client |
| Unit | `LeadIntakeService` creates lead via SDK with correct source | xUnit, mock leads API client |
| Unit | `LeadIntakeService` handles missing property gracefully | xUnit |
| Integration | Webhook endpoint with valid API key creates lead | `WebApplicationFactory` |
| Integration | Webhook endpoint with invalid API key returns 401 | `WebApplicationFactory` |
| Integration | Webhook endpoint with malformed payload returns 400 | `WebApplicationFactory` |
| Integration | Duplicate webhook call does not create duplicate lead | `WebApplicationFactory` |
| Manual | Simulate portal lead submission, verify lead appears in system | Dev environment |

### Security & Privacy

- Webhook endpoints are publicly accessible (portals call them); API key authentication is critical
- API keys must be stored in environment variables, one per portal per tenant
- Incoming payloads contain PII (name, email, phone, message); do NOT log PII content
- Log only: portal name, property reference, timestamp, success/failure
- Rate limiting on webhook endpoints to prevent abuse
- Payload size limit (e.g., 64 KB) to prevent DoS
- HTTPS required for webhook endpoints in production

### TDD Reminder

Write payload parser tests for each portal first with real-world sample payloads. Then write lead intake service tests with mocked dependencies. Finally, write controller/integration tests. Implement to pass.

---

## Task 6.7 -- Frontend Publication Management

**Status:** PENDING
**Dependencies:** 6.5 (publishing worker and API must be available)

### Scope

**In scope:**
- Publication management section on property detail page
- Select which portals to publish to (checkbox per portal)
- View publication status per portal (Published, Pending, Failed, Unpublished)
- Retry failed publications with one click
- Unpublish from specific portals
- Bulk publish: publish a property to all selected portals at once
- View feed URL per portal (for portal configuration)
- Publication history showing attempt timestamps and errors
- Stitch design for publication management panel
- i18n support (en, es)

**Out of scope:**
- Portal configuration/setup UI (admin sets up portal credentials separately)
- Feed URL management (URLs are auto-generated)
- Lead management from this screen (separate in epic P4)

### Acceptance Criteria

- [ ] Property detail page shows a "Portal Publication" section/tab
- [ ] Section displays a card per portal (Kyero, Thribee, SpainHouses) with current status
- [ ] Each portal card shows: portal logo/icon, status badge (color-coded), last published date, feed URL (if published)
- [ ] "Publish" toggle/button for each portal to enable/disable publication
- [ ] "Publish to All" button publishes to all available portals at once
- [ ] "Retry" button appears on Failed publications, triggers retry via API
- [ ] "Unpublish" button appears on Published publications, removes from portal
- [ ] Publication history expandable section per portal showing: attempt date, success/failure, error message (if failed)
- [ ] Status colors: Published = green, Pending = yellow, Failed = red, Draft = gray, Unpublished = gray outlined
- [ ] Feed URL is displayed as a copyable link (click to copy with toast confirmation)
- [ ] Loading states during publish/retry/unpublish actions
- [ ] Error states with user-friendly messages
- [ ] Stitch design exists for publication management panel
- [ ] All components tested with Vitest + RTL
- [ ] Responsive layout

### Implementation Steps

1. Create Stitch design for publication management panel
2. Download Stitch HTML to `.stitch-html/`
3. Create `usePropertyPublications` hook (fetch publications for a property)
4. Create `usePublishProperty` hook (mutation: publish to portal)
5. Create `useUnpublishProperty` hook (mutation)
6. Create `useRetryPublication` hook (mutation)
7. Create `PublicationManagementPanel` component
8. Create `PortalPublicationCard` component (per portal)
9. Create `PublicationStatusBadge` component
10. Create `PublicationHistory` collapsible component
11. Create `FeedUrlCopy` component with clipboard copy
12. Integrate into property detail page as tab or section
13. Add i18n keys
14. Write component tests
15. Verify against Stitch design

### Files to Create/Modify

**Create:**
- `apps/web/src/components/publications/PublicationManagementPanel.tsx`
- `apps/web/src/components/publications/PortalPublicationCard.tsx`
- `apps/web/src/components/publications/PublicationStatusBadge.tsx`
- `apps/web/src/components/publications/PublicationHistory.tsx`
- `apps/web/src/components/publications/FeedUrlCopy.tsx`
- `apps/web/src/hooks/usePropertyPublications.ts`
- `apps/web/src/hooks/usePublishProperty.ts`
- `apps/web/src/hooks/useUnpublishProperty.ts`
- `apps/web/src/hooks/useRetryPublication.ts`
- `apps/web/src/components/publications/__tests__/PublicationManagementPanel.test.tsx`
- `apps/web/src/components/publications/__tests__/PortalPublicationCard.test.tsx`
- `apps/web/src/components/publications/__tests__/PublicationStatusBadge.test.tsx`
- `apps/web/src/components/publications/__tests__/PublicationHistory.test.tsx`
- `.stitch-html/publication-management.html`

**Modify:**
- `apps/web/src/app/[locale]/(dashboard)/properties/[id]/page.tsx` (add Publication section/tab)
- `apps/web/src/messages/en.json` (add publication i18n keys)
- `apps/web/src/messages/es.json` (add publication i18n keys)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `PublicationManagementPanel` renders portal cards for all portals | Vitest + RTL |
| Unit | `PortalPublicationCard` shows correct status badge and actions per status | Vitest + RTL, parameterized |
| Unit | Publish button triggers API call | Vitest + RTL, mock fetch |
| Unit | Retry button triggers API call, shows loading state | Vitest + RTL, mock fetch |
| Unit | Unpublish button shows confirmation, triggers API call | Vitest + RTL |
| Unit | `PublicationHistory` renders attempts with timestamps and errors | Vitest + RTL |
| Unit | `FeedUrlCopy` copies URL to clipboard | Vitest + RTL, mock clipboard API |
| Unit | `PublicationStatusBadge` renders correct color per status | Vitest + RTL |
| Unit | Hooks handle loading, success, error states | Vitest, mock fetch |
| Manual | Full publish/unpublish/retry flow through UI | Dev environment |
| Manual | Visual comparison against Stitch design | Dev environment |

### Security & Privacy

- Feed URLs are displayed to agents; they contain security tokens but are intended to be shared with portals
- Publication management is limited to agents with appropriate permissions
- Error messages from portals may be displayed; ensure they are sanitized

### TDD Reminder

Write component render tests for each status state first. Write interaction tests for publish/retry/unpublish. Then implement components to pass all tests.
