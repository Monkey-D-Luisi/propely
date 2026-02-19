# Propely — Product Roadmap

## Vision

Propely is a **multi-tenant, AI-powered real estate management SaaS** for agencies operating in the Spanish and Portuguese markets. The platform enables agencies to manage properties across branches, leverage AI for data entry and multilingual content generation, capture and nurture leads, schedule appointments, and publish listings to major real estate portals.

**Property is the central entity.** Every other domain concept (contacts, leads, appointments, publications) orbits around properties.

## Target Users

| Role | Description |
|------|-------------|
| Agency Owner | Creates and manages the agency. Sees all branches. Manages billing and global settings. |
| Branch Admin | Manages a single branch (org). Invites agents, configures branch settings. |
| Agent | The real estate professional. Creates and manages properties, contacts, appointments. |
| Viewer | Read-only access. Used for stakeholders, auditors, or temporary access. |

### Organizational Hierarchy

```
Agency (parent entity)
├── Branch A (org/tenant)
│   ├── Agent 1
│   ├── Agent 2
│   └── Viewer 1
├── Branch B (org/tenant)
│   ├── Agent 3
│   └── Admin 1
└── Branch C (org/tenant)
    └── ...
```

- **Agency** is a new entity above the current Organization level
- **Branch** maps to the existing Organization (tenant) concept
- Each branch operates as an isolated tenant with its own data
- Agency owners have cross-branch visibility

## Permission Model

### Fixed Roles

| Role | Scope | Description |
|------|-------|-------------|
| `owner` | Agency | Full access. Manages all branches, billing, agency settings. |
| `admin` | Branch | Manages branch members, settings. Sees all branch data. |
| `agent` | Branch | Manages own properties, contacts, appointments. Visibility scoped to assigned entities by default. |
| `viewer` | Branch | Read-only access to branch data. |

### Permission Overrides

Individual users can receive **permission overrides** that extend or restrict their base role capabilities:

| Permission | Description | Default (agent) |
|-----------|-------------|-----------------|
| `properties.view_all` | View all properties in the branch, not just own | No |
| `properties.edit_all` | Edit any property in the branch | No |
| `contacts.view_all` | View all contacts in the branch | No |
| `contacts.edit_all` | Edit any contact in the branch | No |
| `appointments.view_all` | View all appointments in the branch | No |
| `publishing.manage` | Publish/unpublish properties to portals | No |
| `leads.manage` | Manage leads (assign, convert, close) | Yes |
| `reports.view` | Access dashboards and analytics | No |

Overrides are stored as `(UserId, Permission, Granted: bool)` — a deny override can restrict an admin's default access.

## Architecture

### Microservices

| Service | Port | Purpose | Database |
|---------|------|---------|----------|
| `apps/web` | 3000 | Next.js 16 frontend | — |
| `services/ai-api` | 5010 | AI capabilities (smart-fill, copy generation) | `propely_aiapi` |
| `services/orgs-api` | 5020 | Auth, agencies, branches, teams, billing, permissions | `propely_orgsapi` |
| `services/properties-api` | 5030 | Property management (core domain) | `propely_propertiesapi` |
| `services/contacts-api` | 5050 | Contacts, leads, communication | `propely_contactsapi` |
| `services/appointments-api` | 5060 | Appointments, calendar sync | `propely_appointmentsapi` |
| `services/publishing-api` | 5040 | Portal publication, XML feeds, webhooks | `propely_publishingapi` |

### Inter-Service Communication

Services communicate via **NuGet SDK clients** built with Refit:

| SDK | Published by | Consumed by |
|-----|-------------|-------------|
| `Propely.OrgsApi.Client` | orgs-api | All backend services |
| `Propely.AiApi.Client` | ai-api | properties-api |
| `Propely.PropertiesApi.Client` | properties-api | publishing-api, contacts-api, appointments-api |
| `Propely.ContactsApi.Client` | contacts-api | appointments-api |

Tenant context (`X-Tenant-Id`) is propagated via `TenantDelegatingHandler` in all SDK clients.

### Key Technology Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Property forms | JSON Forms (`@jsonforms/react`) | JSON Schema-driven, supports i18n via `translate`, dynamic form generation |
| Calendar UI | FullCalendar Standard (MIT) | Month/week/day views, drag-and-drop, proven library |
| Inter-service HTTP | Refit + `IHttpClientFactory` + Polly | Declarative, resilient, testable |
| AI model | gpt-5-mini (OpenAI) | Cost-effective for field extraction and copy generation |
| Cloud storage | GCP Cloud Storage | Aligns with existing GCP Cloud Run infrastructure |
| Portal feeds | XML (per-portal XSD) | Industry standard for Kyero, Thribee, SpainHouses |

## Development Methodology

### Use Case-Driven Development

Before implementing any screen or feature:

1. **Define use cases** exhaustively — actor, preconditions, main flow, alternative flows, error flows, postconditions
2. **Design with Stitch MCP** — generate a pixel-perfect design using the use cases as input, producing an HTML reference
3. **Implement with TDD** — write failing tests first (unit + integration), then implement until green
4. **Verify** — screenshot comparison against Stitch design, full test suite green

This flow is **mandatory** and encoded in the task workflow.

### Test-Driven Development (TDD)

Every use case follows Red-Green-Refactor:

1. **Red**: Write failing tests that capture the acceptance criteria
2. **Green**: Write the minimum code to make tests pass
3. **Refactor**: Clean up while keeping tests green

Coverage targets:
- Domain layer: >90%
- Application layer: >80%
- Infrastructure layer: >60%
- API / Presentation layer: >50%
- Frontend components: >70%

---

## Phase 0 — Foundation

**Goal:** Prepare the repository for Propely development. Rename, clean up, update infrastructure.

### Task 0.1 — Documentation & Roadmap Overhaul
- **Status:** IN_PROGRESS
- **Scope:** Rewrite all documentation, agent instructions, roadmap, and backlog for Propely. Clean up template-specific content.
- **Deliverables:** This roadmap, updated agent instructions, new backlog epics, cleaned docs.

### Task 0.2 — Rename SaasTemplate → Propely
- **Status:** PENDING
- **Dependencies:** 0.1
- **Scope:** Rename all namespaces, solution files, project files, and references from `SaasTemplate.*` to `Propely.*`. Update docker-compose, database names, CI pipelines, scripts, .env files.
- **Deliverables:** All code compiles and tests pass with `Propely.*` naming.

### Task 0.3 — Scaffold New Service Solutions
- **Status:** PENDING
- **Dependencies:** 0.2
- **Scope:** Create solution scaffolding for the 4 new microservices (properties-api, contacts-api, appointments-api, publishing-api). Each gets the Clean Architecture 4-layer structure + test projects. No domain logic yet — just compilable empty shells.
- **Deliverables:** 4 new `.sln` files, 4×6 projects (Domain, Application, Infrastructure, Api, UnitTests, IntegrationTests), architecture tests enforcing layer dependencies.

### Task 0.4 — NuGet SDK Client Infrastructure
- **Status:** PENDING
- **Dependencies:** 0.3
- **Scope:** Create the `Propely.<Service>.Client` project pattern with Refit interfaces, `TenantDelegatingHandler`, Polly retry policies, and `IServiceCollection` extensions. Implement for `orgs-api` first as the reference.
- **Deliverables:** `Propely.OrgsApi.Client` package, consumption example, integration test verifying tenant header propagation.

### Task 0.5 — Docker Compose & Infrastructure Updates
- **Status:** PENDING
- **Dependencies:** 0.3
- **Scope:** Update `docker-compose.yml` to include all 6 services. Update database init script to create all 6 databases. Add new run scripts for each service. Update `.env.example`.
- **Deliverables:** `docker-compose up` starts all services. Individual run scripts work. Health endpoints respond.

### Task 0.6 — CI Pipeline Updates
- **Status:** PENDING
- **Dependencies:** 0.5
- **Scope:** Update GitHub Actions CI to build and test all 6 services. Update deploy workflows. Add per-service build matrix.
- **Deliverables:** CI pipeline builds and tests all services. Deployment targets updated.

---

## Phase 1 — Agency Hierarchy & Permissions

**Goal:** Extend orgs-api with agency hierarchy and granular permission system. This is foundational — all subsequent phases depend on it.

### Task 1.1 — Agency Domain Model
- **Status:** PENDING
- **Dependencies:** 0.2
- **Scope:** Add `Agency` entity to orgs-api Domain layer. An Agency owns one or more Organizations (branches). Define the aggregate, value objects, and domain events (`AgencyCreatedV1`, `BranchAddedToAgencyV1`).
- **TDD:** Domain entity tests (creation, validation, invariants).

### Task 1.2 — Agency Persistence & API
- **Status:** PENDING
- **Dependencies:** 1.1
- **Scope:** EF Core configuration for Agency. Migrations. API endpoints: create agency, list agencies for user, add branch to agency, get agency details. Update Organization to include optional `AgencyId` FK.
- **TDD:** Integration tests for all endpoints. Repository tests.

### Task 1.3 — Permission System Domain Model
- **Status:** PENDING
- **Dependencies:** 1.1
- **Scope:** Define `Permission` enum, `PermissionOverride` entity (UserId, Permission, Granted), and `IPermissionEvaluator` interface. Add default permission matrix per role. Domain tests for permission resolution logic (base role + overrides).
- **TDD:** Unit tests for every combination: agent + override grant, admin + override deny, owner (all permissions).

### Task 1.4 — Permission Persistence & Authorization Policies
- **Status:** PENDING
- **Dependencies:** 1.3
- **Scope:** EF Core for PermissionOverride. ASP.NET Core authorization policies that evaluate permissions dynamically (not hardcoded role checks). Middleware/handler to load user permissions. API endpoints: list permissions for user, set override, remove override.
- **TDD:** Integration tests verifying authorization enforcement at API level.

### Task 1.5 — Orgs-API NuGet SDK Client
- **Status:** PENDING
- **Dependencies:** 1.4, 0.4
- **Scope:** Create `Propely.OrgsApi.Client` with Refit interfaces for: get user permissions, validate membership, get agency/branch hierarchy. Other services will use this to enforce authorization.
- **TDD:** Contract tests verifying SDK matches API behavior.

### Task 1.6 — Frontend: Agency Management
- **Status:** PENDING
- **Dependencies:** 1.2
- **Scope:**
  1. Use case definition for agency management screens
  2. Stitch MCP design generation
  3. Implementation: agency creation flow, branch switcher in header, agency settings page
- **TDD:** Component tests for agency screens. E2E test for agency creation flow.

### Task 1.7 — Frontend: Permission Management UI
- **Status:** PENDING
- **Dependencies:** 1.4, 1.6
- **Scope:**
  1. Use case definition for permission management screens
  2. Stitch MCP design generation
  3. Implementation: member permissions page (view role, view/edit overrides), admin-only UI
- **TDD:** Component tests for permission UI. Integration test for override CRUD.

---

## Phase 2 — Properties Microservice

**Goal:** Build the core domain — property management with full CRUD, lifecycle, media, multilingual descriptions, and JSON Forms.

### Task 2.1 — Property Domain Model
- **Status:** PENDING
- **Dependencies:** 0.3
- **Scope:** Define `Property` aggregate root with:
  - Value objects: `PropertyType` (Apartment, House, Villa, Townhouse, Penthouse, Studio, Office, Retail, Warehouse, Land, NewConstruction), `OperationType` (Sale, Rent, VacationRental, RentToBuy), `PropertyStatus` (Draft, Active, Reserved, Sold, Rented, Archived), `Address`, `GeoLocation`, `Price`, `Area`
  - Properties: title, reference code, type, operation, status, address, geo, price, area (built/usable/plot), rooms, bathrooms, floor, has elevator, has parking, has pool, has garden, energy certificate, year built, description (multi-lang), features list
  - Behavior: `Activate()`, `Reserve()`, `MarkSold()`, `MarkRented()`, `Archive()`, `Reactivate()` with state machine validation
  - Events: `PropertyCreatedV1`, `PropertyUpdatedV1`, `PropertyStatusChangedV1`, `PropertyArchivedV1`
  - Agent assignment: `AssignedAgentId` (FK to user)
  - Tenant isolation: `BranchId` (FK to org)
- **TDD:** Exhaustive unit tests for state transitions, invariants, validation.

### Task 2.2 — Property Persistence (EF Core)
- **Status:** PENDING
- **Dependencies:** 2.1
- **Scope:** EF Core DbContext, entity configurations (owned types for value objects), migrations, repository implementations, query specifications for filtering/sorting/pagination.
- **TDD:** Integration tests with Testcontainers PostgreSQL.

### Task 2.3 — Property CRUD API
- **Status:** PENDING
- **Dependencies:** 2.2, 1.5
- **Scope:** CQRS commands and queries: CreateProperty, UpdateProperty, DeleteProperty (soft), ChangePropertyStatus, AssignAgent, ListProperties (paginated, filtered, sorted), GetPropertyById. Authorization via orgs-api SDK (agent sees own, admin/owner sees all, viewer reads only).
- **TDD:** Integration tests for every endpoint + authorization scenarios (agent owns, agent doesn't own, admin, viewer).

### Task 2.4 — Multi-Language Descriptions
- **Status:** PENDING
- **Dependencies:** 2.1
- **Scope:** `PropertyDescription` value object with `Dictionary<string, LocalizedContent>` where key is ISO 639-1 code (es, en, fr, de, ...). `LocalizedContent` contains title, description, and feature highlights. Spanish is the default but none are mandatory beyond at least one.
- **TDD:** Unit tests for localized content validation, serialization.

### Task 2.5 — Media Management
- **Status:** PENDING
- **Dependencies:** 2.3
- **Scope:**
  - `PropertyMedia` entity: id, propertyId, type (Photo, FloorPlan, Document, Video, VirtualTour), url, thumbnailUrl, sortOrder, caption, mimeType, sizeBytes
  - For Photo/FloorPlan/Document: upload to GCP Cloud Storage, generate thumbnails
  - For Video/VirtualTour: store external URL only (YouTube, Matterport)
  - API: upload media, reorder, delete, set primary photo
  - Storage abstraction: `IFileStorageService` with GCP implementation + local filesystem for dev
- **TDD:** Unit tests for media validation. Integration tests for upload/download. Mock storage for unit tests.

### Task 2.6 — Property Lifecycle State Machine
- **Status:** PENDING
- **Dependencies:** 2.3
- **Scope:** Enforce valid state transitions via domain logic:
  ```
  Draft → Active (requires: at least 1 photo, title in at least 1 language, price, type, operation)
  Active → Reserved
  Active → Archived
  Reserved → Active (unreserve)
  Reserved → Sold (if operation=Sale/RentToBuy)
  Reserved → Rented (if operation=Rent/VacationRental/RentToBuy)
  Sold → Archived
  Rented → Archived
  Rented → Active (re-list)
  Archived → Draft (re-open)
  ```
  Emit `PropertyStatusChangedV1` event on every transition.
- **TDD:** Unit tests for every valid transition, every invalid transition, and prerequisite validation.

### Task 2.7 — JSON Forms Schema & Frontend Setup
- **Status:** PENDING
- **Dependencies:** 2.3
- **Scope:**
  1. Define JSON Schema for property data entry (all fields from 2.1)
  2. Define UI Schema for layout and conditional visibility
  3. Set up `@jsonforms/react` + `@jsonforms/material-renderers` in the web app
  4. Create custom renderers for: multi-language text, media upload, geo picker, price with currency
  5. i18n integration via JSON Forms `translate` function
- **TDD:** Component tests for custom renderers. Schema validation tests.

### Task 2.8 — Frontend: Property List & Detail
- **Status:** PENDING
- **Dependencies:** 2.7, 1.6
- **Scope:**
  1. Use case definition: list properties (filter by status, type, operation, agent), view property detail
  2. Stitch MCP design generation
  3. Implementation: property list with filters/pagination, property detail page with gallery, map, descriptions
- **TDD:** Component tests. E2E test for list → detail navigation.

### Task 2.9 — Frontend: Property Create & Edit
- **Status:** PENDING
- **Dependencies:** 2.7, 2.8
- **Scope:**
  1. Use case definition: create property (JSON Forms wizard), edit property
  2. Stitch MCP design generation
  3. Implementation: multi-step form with JSON Forms, media upload, map picker, preview before save
- **TDD:** Component tests for form steps. Integration test for full creation flow.

### Task 2.10 — Properties-API NuGet SDK Client
- **Status:** PENDING
- **Dependencies:** 2.3
- **Scope:** Create `Propely.PropertiesApi.Client` with Refit interfaces: get property by ID, list properties (filtered), get property media. Used by publishing-api, contacts-api, appointments-api.
- **TDD:** Contract tests.

---

## Phase 3 — AI Smart-Fill & Content Generation

**Goal:** Enable agents to populate property forms from free text or photos using AI, and generate multilingual property descriptions.

### Task 3.1 — AI Field Extraction from Text
- **Status:** PENDING
- **Dependencies:** 2.1, 0.4
- **Scope:** Extend ai-api with `POST /api/properties/extract-from-text` endpoint. Accepts free-text property description → returns structured JSON matching the property JSON Schema. Uses gpt-5-mini with a carefully engineered system prompt. Returns confidence scores per field.
- **TDD:** Unit tests with mocked OpenAI (fixed responses). Integration test with real API (optional, behind flag).

### Task 3.2 — AI Field Extraction from Photos
- **Status:** PENDING
- **Dependencies:** 3.1, 2.5
- **Scope:** `POST /api/properties/extract-from-photos` endpoint. Accepts one or more property photos → uses gpt-5-mini vision to extract: property type, room count, features (pool, garden, parking), approximate area, condition, style. Returns partial structured JSON with confidence scores.
- **TDD:** Unit tests with mocked vision API. Test with sample property photos.

### Task 3.3 — AI Copy Generation
- **Status:** PENDING
- **Dependencies:** 3.1
- **Scope:** `POST /api/properties/generate-copy` endpoint. Accepts structured property data + target languages → generates marketing descriptions in each language. Supports tone options (professional, casual, luxury). Returns `Dictionary<string, LocalizedContent>`.
- **TDD:** Unit tests with mocked API. Test for multiple language outputs.

### Task 3.4 — AI Prompt Engineering & Tuning
- **Status:** PENDING
- **Dependencies:** 3.1, 3.2, 3.3
- **Scope:** Refine system prompts for Spanish real estate terminology, Idealista/Fotocasa style conventions, energy certificate codes, cadastral reference formats. Create prompt test suite with real property descriptions and expected outputs. Measure extraction accuracy.
- **TDD:** Prompt regression test suite (input descriptions → expected field extractions).

### Task 3.5 — AI-API NuGet SDK Client Enhancement
- **Status:** PENDING
- **Dependencies:** 3.1, 3.2, 3.3
- **Scope:** Extend `Propely.AiApi.Client` with Refit interfaces for text extraction, photo extraction, and copy generation endpoints. Add typed request/response DTOs.
- **TDD:** Contract tests.

### Task 3.6 — Frontend: Smart-Fill UX
- **Status:** PENDING
- **Dependencies:** 3.5, 2.9
- **Scope:**
  1. Use case definition: paste text → preview extracted fields → accept/modify → save. Upload photos → preview extracted data → merge with text extraction → review → save.
  2. Stitch MCP design generation
  3. Implementation: smart-fill panel in property create form, confidence indicators per field, diff view for review, photo upload zone with extraction progress.
- **TDD:** Component tests for smart-fill panel. E2E test for full text→extract→review→save flow.

---

## Phase 4 — Contacts & Leads

**Goal:** Build the contacts domain with dual-role contacts, lead management, and property linkage.

### Task 4.1 — Contact Domain Model
- **Status:** PENDING
- **Dependencies:** 0.3
- **Scope:** Define `Contact` aggregate root:
  - Fields: name, email, phone, secondaryPhone, address, preferredLanguage, source (Portal, WalkIn, Referral, Website, Phone, Other), notes
  - Value objects: `ContactRole` flags enum (Buyer, Seller, Tenant, Landlord, Professional), `ContactSource`
  - A contact can have multiple roles simultaneously
  - Agent assignment: `AssignedAgentId`
  - Tenant isolation: `BranchId`
  - Events: `ContactCreatedV1`, `ContactUpdatedV1`
  - Relationship: `ContactPropertyInterest` linking a contact to properties they're interested in (with interest level, notes, date)
- **TDD:** Unit tests for contact creation, role management, property interest linkage.

### Task 4.2 — Lead Domain Model
- **Status:** PENDING
- **Dependencies:** 4.1, 2.1
- **Scope:** Define `Lead` entity:
  - Fields: name, email, phone, message, source, sourcePortal, sourceUrl, propertyId (REQUIRED — leads always link to a property), status (New, Contacted, Qualified, Converted, Lost), assignedAgentId, convertedContactId (nullable)
  - Behavior: `MarkContacted()`, `Qualify()`, `Convert(contactId)`, `MarkLost(reason)`
  - Events: `LeadReceivedV1`, `LeadStatusChangedV1`, `LeadConvertedV1`
  - Deduplication: by email + propertyId (prevent duplicate leads for same person on same property)
- **TDD:** Unit tests for lead state machine, deduplication rules, conversion logic.

### Task 4.3 — Contacts & Leads Persistence & API
- **Status:** PENDING
- **Dependencies:** 4.1, 4.2
- **Scope:** EF Core setup for contacts-api. CRUD endpoints for contacts and leads. Lead assignment (manual + auto-assign to property agent). Contact search/filter. Lead pipeline view (grouped by status). Authorization: agent sees own, admin sees all.
- **TDD:** Integration tests for all endpoints + authorization.

### Task 4.4 — Lead Conversion Flow
- **Status:** PENDING
- **Dependencies:** 4.3
- **Scope:** When a lead is converted: create Contact (or link to existing by email), copy lead data, maintain lead→contact relationship. If contact already exists, merge data (don't duplicate). Emit `LeadConvertedV1` event.
- **TDD:** Unit tests for conversion logic. Integration tests for merge scenarios.

### Task 4.5 — Contacts-API NuGet SDK Client
- **Status:** PENDING
- **Dependencies:** 4.3
- **Scope:** `Propely.ContactsApi.Client` with Refit interfaces: get contact, list contacts, get leads for property, create lead (used by publishing-api for portal webhooks).
- **TDD:** Contract tests.

### Task 4.6 — Frontend: Contacts List & Detail
- **Status:** PENDING
- **Dependencies:** 4.3, 1.6
- **Scope:**
  1. Use case definition: list contacts, filter by role/source/agent, view contact detail with property interests and communication history
  2. Stitch MCP design generation
  3. Implementation
- **TDD:** Component tests. E2E test for list → detail.

### Task 4.7 — Frontend: Lead Pipeline
- **Status:** PENDING
- **Dependencies:** 4.3, 4.6
- **Scope:**
  1. Use case definition: view leads by status (kanban or list), assign lead, convert lead, mark lost
  2. Stitch MCP design generation
  3. Implementation: lead pipeline view with drag-and-drop status changes, lead detail panel, conversion modal
- **TDD:** Component tests for pipeline UI. Integration test for status change flow.

---

## Phase 5 — Appointments & Calendar

**Goal:** Enable scheduling of property viewings, meetings, and generic appointments with bidirectional calendar sync.

### Task 5.1 — Appointment Domain Model
- **Status:** PENDING
- **Dependencies:** 0.3
- **Scope:** Define `Appointment` aggregate root:
  - Types: `AppointmentType` (PropertyViewing, OwnerMeeting, Generic)
  - Fields: type, title, description, startUtc, endUtc, location, propertyId (nullable — required for viewings), contactId (nullable), agentId (required), status (Scheduled, Confirmed, Completed, Cancelled, NoShow), notes, externalCalendarEventId
  - Behavior: `Confirm()`, `Complete(notes)`, `Cancel(reason)`, `MarkNoShow()`
  - Events: `AppointmentScheduledV1`, `AppointmentStatusChangedV1`, `AppointmentCancelledV1`
  - Tenant isolation: `BranchId`
- **TDD:** Unit tests for appointment lifecycle, validation (end > start, viewing requires property).

### Task 5.2 — Appointments Persistence & API
- **Status:** PENDING
- **Dependencies:** 5.1
- **Scope:** EF Core setup for appointments-api. CRUD endpoints. List appointments by agent/date range/property/contact. Authorization: agent sees own, admin sees all.
- **TDD:** Integration tests for all endpoints + authorization.

### Task 5.3 — Google Calendar Integration
- **Status:** PENDING
- **Dependencies:** 5.2
- **Scope:** OAuth2 flow for Google Calendar. Bidirectional sync: create/update/delete Propely appointment → push to Google Calendar. Google Calendar webhook for external changes → update Propely. Store sync state (externalCalendarEventId, lastSyncedAt). Handle conflicts.
- **TDD:** Unit tests with mocked Google API. Integration tests for sync scenarios.

### Task 5.4 — Outlook/Microsoft Graph Integration
- **Status:** PENDING
- **Dependencies:** 5.2
- **Scope:** OAuth2 flow for Microsoft Graph. Same bidirectional sync pattern as Google Calendar. `ICalendarSyncService` interface with Google and Microsoft implementations.
- **TDD:** Unit tests with mocked Graph API.

### Task 5.5 — Calendar Sync Engine
- **Status:** PENDING
- **Dependencies:** 5.3, 5.4
- **Scope:** Background service that processes sync queue. Handles rate limiting, retry with backoff, conflict resolution (last-write-wins with user notification). Publishes `AppointmentSyncedV1` events.
- **TDD:** Unit tests for conflict resolution. Integration tests for sync queue processing.

### Task 5.6 — Frontend: Calendar View
- **Status:** PENDING
- **Dependencies:** 5.2, 1.6
- **Scope:**
  1. Use case definition: view calendar (month/week/day), create appointment from calendar, drag to reschedule, click for details
  2. Stitch MCP design generation
  3. Implementation with FullCalendar: month/week/day views, event coloring by type, drag-and-drop reschedule, appointment detail popover
- **TDD:** Component tests for calendar wrapper. E2E test for create/view/reschedule flow.

### Task 5.7 — Frontend: Appointment Booking
- **Status:** PENDING
- **Dependencies:** 5.6
- **Scope:**
  1. Use case definition: book viewing from property detail page, book meeting from contact detail, agent availability check
  2. Stitch MCP design generation
  3. Implementation: booking modal with date/time picker, contact selection, property selection (for viewings)
- **TDD:** Component tests. Integration test for booking flow.

---

## Phase 6 — Portal Publication

**Goal:** Publish property listings to major real estate portals via XML feeds and receive leads via webhooks. This phase is a strong market differentiator but can ship after the core MVP.

### Task 6.1 — Publishing Domain Model
- **Status:** PENDING
- **Dependencies:** 0.3
- **Scope:** Define `Publication` entity:
  - Fields: propertyId, portal (Kyero, Thribee, SpainHouses), status (Pending, Published, Failed, Removed), lastPublishedAt, externalId, feedUrl, errors
  - `PortalAdapter` interface: `GenerateXml(property)`, `Validate(property)`, `GetRequiredFields()`
  - Events: `PropertyPublishedV1`, `PropertyUnpublishedV1`, `PublicationFailedV1`
- **TDD:** Unit tests for publication lifecycle, validation per portal.

### Task 6.2 — Portal Adapters: Kyero
- **Status:** PENDING
- **Dependencies:** 6.1, 2.10
- **Scope:** Implement Kyero XML v3.9 adapter. Map Propely property model → Kyero XML schema. Handle required fields, media URLs, feature mapping. Validate against Kyero XSD.
- **TDD:** Unit tests with sample properties → expected XML output. XSD validation tests.

### Task 6.3 — Portal Adapters: Thribee
- **Status:** PENDING
- **Dependencies:** 6.1, 2.10
- **Scope:** Implement Thribee XML adapter (covers Trovit, Mitula, Nestoria, Nuroa). Map property model → Thribee XML schema.
- **TDD:** Unit tests with sample properties → expected XML output.

### Task 6.4 — Portal Adapters: SpainHouses
- **Status:** PENDING
- **Dependencies:** 6.1, 2.10
- **Scope:** Implement SpainHouses.net XML + XSD adapter. Spain-domestic focus.
- **TDD:** Unit tests with sample properties → expected XML output. XSD validation.

### Task 6.5 — Publishing Worker
- **Status:** PENDING
- **Dependencies:** 6.2, 6.3, 6.4
- **Scope:** Background worker that listens for `PropertyStatusChangedV1` events. When a property is activated and has portal assignments, generates XML feed and publishes. Handles retries, error reporting. Periodic re-sync job.
- **TDD:** Integration tests for event → XML generation → publish flow.

### Task 6.6 — Lead Intake Webhooks
- **Status:** PENDING
- **Dependencies:** 6.1, 4.5
- **Scope:** Webhook endpoints for each portal. Parse incoming lead data, deduplicate, create lead in contacts-api via SDK (lead always linked to the published property). Handle different portal lead formats.
- **TDD:** Integration tests with sample webhook payloads for each portal.

### Task 6.7 — Frontend: Publication Management
- **Status:** PENDING
- **Dependencies:** 6.5, 2.8
- **Scope:**
  1. Use case definition: select portals for a property, view publication status, retry failed, bulk publish/unpublish
  2. Stitch MCP design generation
  3. Implementation: publication panel in property detail, portal status badges, bulk actions in property list
- **TDD:** Component tests. E2E test for publish/unpublish flow.

---

## Phase 7 — Polish & Analytics

**Goal:** Dashboard, analytics, design refinement, and additional features.

### Task 7.1 — Dashboard & Analytics
- **Status:** PENDING
- **Dependencies:** 2.3, 4.3, 5.2
- **Scope:**
  1. Use case definition: agent dashboard (my properties by status, my leads, upcoming appointments), admin dashboard (branch metrics, agent performance)
  2. Stitch MCP design generation
  3. Implementation: dashboard pages with charts and KPIs
- **TDD:** Component tests. API tests for analytics endpoints.

### Task 7.2 — AI Property Valuation (Experimental)
- **Status:** PENDING
- **Dependencies:** 3.4
- **Scope:** AI endpoint that estimates property value based on characteristics, location, and comparable properties. Experimental feature — clearly marked as estimate, not professional valuation.
- **TDD:** Unit tests with mocked AI. Sanity checks for output ranges.

### Task 7.3 — Additional Portal Adapters
- **Status:** PENDING
- **Dependencies:** 6.5
- **Scope:** Add adapters for Idealista, Fotocasa, Habitaclia (if API access is available — these portals may require partnership agreements).
- **TDD:** Same pattern as Phase 6 adapters.

### Task 7.4 — Stitch Design Refinement Pass
- **Status:** PENDING
- **Dependencies:** 7.1
- **Scope:** Full Stitch design audit of all screens. Generate updated designs. Pixel-perfect alignment pass. Ensure consistent design language across all pages.
- **TDD:** Visual regression tests.

---

## Dependency Graph (Summary)

```
Phase 0 ──┬── 0.1 Documentation
           ├── 0.2 Rename ──────┬── 0.3 Scaffold ──── 0.4 NuGet SDK
           │                    │                      │
           │                    │                      ├── 0.5 Docker
           │                    │                      │   └── 0.6 CI
           │                    ▼                      ▼
Phase 1 ──┼── 1.1 Agency ───── 1.2 Agency API ─── 1.6 Agency UI
           │   └── 1.3 Perms ── 1.4 Perms API ─── 1.7 Perms UI
           │                    └── 1.5 Orgs SDK
           │
Phase 2 ──┼── 2.1 Property ─── 2.2 EF Core ──── 2.3 CRUD API ── 2.10 SDK
           │   └── 2.4 i18n     └── 2.5 Media   │
           │                    └── 2.6 Lifecycle │
           │                    └── 2.7 JSON Forms ── 2.8 List UI ── 2.9 Create UI
           │
Phase 3 ──┼── 3.1 Text Extract ─── 3.2 Photo Extract
           │   └── 3.3 Copy Gen ─── 3.4 Prompts ── 3.5 SDK ── 3.6 UI
           │
Phase 4 ──┼── 4.1 Contact ──── 4.2 Lead ──── 4.3 API ──── 4.5 SDK
           │                                  └── 4.4 Conversion
           │                                  └── 4.6 Contact UI ── 4.7 Lead UI
           │
Phase 5 ──┼── 5.1 Appointment ── 5.2 API ──── 5.3 Google ─┐
           │                                   5.4 Outlook ─┤── 5.5 Sync
           │                                   └── 5.6 Calendar UI ── 5.7 Booking
           │
Phase 6 ──┼── 6.1 Publishing ── 6.2 Kyero ─┐
           │                     6.3 Thribee ├── 6.5 Worker ── 6.7 UI
           │                     6.4 Spain ──┘   └── 6.6 Webhooks
           │
Phase 7 ──┴── 7.1 Dashboard ── 7.2 Valuation
                                7.3 Adapters ── 7.4 Design
```

## Portal Targets

| Portal | Format | Market | Notes |
|--------|--------|--------|-------|
| Kyero | XML v3.9 | Spain, Portugal (international buyers) | English-language portal, most international traffic |
| Thribee | XML | Multi-country (Trovit, Mitula, Nestoria, Nuroa) | Aggregator covering 4 portals with one feed |
| SpainHouses.net | XML + XSD | Spain (domestic) | XSD validation required |
| Idealista | API (future) | Spain, Portugal, Italy | Requires partnership, Phase 7 |
| Fotocasa | API (future) | Spain | Requires partnership, Phase 7 |

## Key Library Decisions

| Library | Version | Purpose | License |
|---------|---------|---------|---------|
| `@jsonforms/react` | latest | Schema-driven property forms | MIT |
| `@jsonforms/material-renderers` | latest | Standard renderer set | MIT |
| FullCalendar Standard | latest | Calendar UI for appointments | MIT |
| Refit | latest | Declarative HTTP clients for inter-service | MIT |
| Polly | latest | Resilience (retry, circuit breaker) | BSD-3 |
| Google.Apis.Calendar.v3 | latest | Google Calendar integration | Apache 2.0 |
| Microsoft.Graph | latest | Outlook calendar integration | MIT |
| Google.Cloud.Storage.V1 | latest | GCP Cloud Storage for media | Apache 2.0 |
