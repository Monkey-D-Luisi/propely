# Propely — Product Roadmap

## Vision

Propely is a **multi-tenant, AI-powered real estate management SaaS** for agencies operating in the Spanish and Portuguese markets. The platform's core differentiator is **natural language process automation**: agents interact with the system through text and voice commands to create properties, manage leads, schedule appointments, close operations, and perform any system action — without navigating menus or filling forms manually.

**The AI is the interface.** Instead of "AI helps fill a form," the vision is "the user speaks or types what they need, and the system executes it."

**Property is the central entity.** Every other domain concept (contacts, leads, appointments) orbits around properties.

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
| `leads.manage` | Manage leads (assign, convert, close) | Yes |
| `reports.view` | Access dashboards and analytics | No |

Overrides are stored as `(UserId, Permission, Granted: bool)` — a deny override can restrict an admin's default access.

## Architecture

### Microservices

| Service | Port | Purpose | Database |
|---------|------|---------|----------|
| `apps/web` | 3000 | Next.js 16 frontend | — |
| `services/ai-api` | 5010 | AI action engine — NL intent classification, action execution, voice transcription, copy generation | `propely_aiapi` |
| `services/orgs-api` | 5020 | Auth, agencies, branches, teams, billing, permissions | `propely_orgsapi` |
| `services/properties-api` | 5030 | Property management (core domain) | `propely_propertiesapi` |
| `services/contacts-api` | 5050 | Contacts, leads, communication | `propely_contactsapi` |
| `services/appointments-api` | 5060 | Appointments, calendar sync | `propely_appointmentsapi` |
| `services/publishing-api` | 5040 | Portal publication (deprioritized — scaffolded, no active tasks) | `propely_publishingapi` |

### AI Action Engine Architecture

The `ai-api` service is the orchestrator for all natural language interactions:

```
User Input (text or voice)
        │
        ▼
┌─────────────────────────────┐
│  Voice Input?                │
│  ── gpt-4o-mini-transcribe ─┤── STT (Speech-to-Text)
│     via OpenAI Audio API     │
└──────────┬──────────────────┘
           │ (text)
           ▼
┌─────────────────────────────┐
│  Intent Classifier           │
│  ── OpenAI function calling ─┤── Classifies intent + extracts entities
│     with tool definitions    │   "Create a 3-bed apartment in Malaga for 250k"
│                              │   → intent: CreateProperty
│                              │   → entities: {bedrooms: 3, type: Apartment, city: Malaga, price: 250000}
└──────────┬──────────────────┘
           │
           ▼
┌─────────────────────────────┐
│  Action Router               │
│  ── IActionHandler<T>        │── Dispatches to typed handler via DI
│     pattern (MediatR)        │
└──────────┬──────────────────┘
           │
           ▼
┌─────────────────────────────────────────────────────┐
│  Action Handlers (one per action type)               │
│                                                      │
│  CreatePropertyHandler  → PropertiesApi SDK          │
│  CreateLeadHandler      → ContactsApi SDK            │
│  BookViewingHandler     → AppointmentsApi SDK        │
│  QueryPropertyHandler   → PropertiesApi SDK          │
│  GenerateCopyHandler    → OpenAI (copy generation)   │
│  ReservePropertyHandler → PropertiesApi SDK          │
│  CloseOperationHandler  → PropertiesApi SDK          │
│  ...extensible via IActionHandler<T> registration    │
└──────────┬──────────────────────────────────────────┘
           │
           ▼
┌─────────────────────────────┐
│  Response Builder            │
│  ── Structured result +      │── Returns action result + NL confirmation
│     NL confirmation text     │   "Created apartment AP-2024-001 in Malaga"
└─────────────────────────────┘
```

### Inter-Service Communication

Services communicate via **NuGet SDK clients** built with Refit:

| SDK | Published by | Consumed by |
|-----|-------------|-------------|
| `Propely.OrgsApi.Client` | orgs-api | All backend services |
| `Propely.AiApi.Client` | ai-api | web (via HTTP), properties-api |
| `Propely.PropertiesApi.Client` | properties-api | ai-api, contacts-api, appointments-api |
| `Propely.ContactsApi.Client` | contacts-api | ai-api, appointments-api |
| `Propely.AppointmentsApi.Client` | appointments-api | ai-api |

Tenant context (`X-Tenant-Id`) is propagated via `TenantDelegatingHandler` in all SDK clients.

**Key change:** `ai-api` now **consumes** the SDK clients of other services (properties, contacts, appointments) to execute actions on behalf of the user.

### Key Technology Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| NL action classification | OpenAI function calling / tool use | Native structured output, reliable intent+entity extraction |
| Voice transcription | `gpt-4o-mini-transcription` (OpenAI Audio API) | Cost-effective, high-quality STT for Spanish and English |
| Property forms | JSON Forms (`@jsonforms/react`) | JSON Schema-driven, supports i18n, dynamic form generation |
| Calendar UI | FullCalendar Standard (MIT) | Month/week/day views, drag-and-drop |
| Inter-service HTTP | Refit + `IHttpClientFactory` + Polly | Declarative, resilient, testable |
| AI model (general) | gpt-5-mini (OpenAI) | Cost-effective for intent classification and content generation |
| Cloud storage | GCP Cloud Storage | Aligns with existing GCP Cloud Run infrastructure |
| Command bar UI | cmdk (React) | Lightweight, accessible command palette |
| Portal field reference | Kyero/SpainHouses/Thribee XSD schemas | Used as reference for property model fields, not for publication |

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
- **Status:** DONE
- **Scope:** Rewrite all documentation, agent instructions, roadmap, and backlog for Propely. Clean up template-specific content.
- **Deliverables:** This roadmap, updated agent instructions, new backlog epics, cleaned docs.

### Task 0.2 — Rename SaasTemplate → Propely
- **Status:** DONE
- **Dependencies:** 0.1
- **Scope:** Rename all namespaces, solution files, project files, and references from `SaasTemplate.*` to `Propely.*`. Update docker-compose, database names, CI pipelines, scripts, .env files.
- **Deliverables:** All code compiles and tests pass with `Propely.*` naming.

### Task 0.3 — Scaffold New Service Solutions
- **Status:** DONE
- **Dependencies:** 0.2
- **Scope:** Create solution scaffolding for the 4 new microservices (properties-api, contacts-api, appointments-api, publishing-api). Each gets the Clean Architecture 4-layer structure + test projects. No domain logic yet — just compilable empty shells.
- **Deliverables:** 4 new `.sln` files, 4×6 projects (Domain, Application, Infrastructure, Api, UnitTests, IntegrationTests), architecture tests enforcing layer dependencies.

### Task 0.4 — NuGet SDK Client Infrastructure
- **Status:** DONE
- **Dependencies:** 0.3
- **Scope:** Create the `Propely.<Service>.Client` project pattern with Refit interfaces, `TenantDelegatingHandler`, Polly retry policies, and `IServiceCollection` extensions. Implement for `orgs-api` first as the reference.
- **Deliverables:** `Propely.OrgsApi.Client` package, consumption example, integration test verifying tenant header propagation.

### Task 0.5 — Docker Compose & Infrastructure Updates
- **Status:** DONE
- **Dependencies:** 0.3
- **Scope:** Update `docker-compose.yml` to include all 6 services. Update database init script to create all 6 databases. Add new run scripts for each service. Update `.env.example`.
- **Deliverables:** `docker-compose up` starts all services. Individual run scripts work. Health endpoints respond.

### Task 0.6 — CI Pipeline Updates
- **Status:** DONE
- **Dependencies:** 0.5
- **Scope:** Update GitHub Actions CI to build and test all 6 services. Update deploy workflows. Add per-service build matrix.
- **Deliverables:** CI pipeline builds and tests all services. Deployment targets updated.

---

## Phase 1 — Agency Hierarchy & Permissions

**Goal:** Extend orgs-api with agency hierarchy and granular permission system. This is foundational — all subsequent phases depend on it.

### Task 1.1 — Agency Domain Model
- **Status:** DONE
- **Dependencies:** 0.2
- **Scope:** Add `Agency` entity to orgs-api Domain layer. An Agency owns one or more Organizations (branches). Define the aggregate, value objects, and domain events (`AgencyCreatedV1`, `BranchAddedToAgencyV1`).
- **TDD:** Domain entity tests (creation, validation, invariants).

### Task 1.2 — Agency Persistence & API
- **Status:** DONE
- **Dependencies:** 1.1
- **Scope:** EF Core configuration for Agency. Migrations. API endpoints: create agency, list agencies for user, add branch to agency, get agency details. Update Organization to include optional `AgencyId` FK.
- **TDD:** Integration tests for all endpoints. Repository tests.
- **Note:** Merged into a single implementation pass with Task 1.1 (task 0007).

### Task 1.3 — Permission System Domain Model
- **Status:** DONE
- **Dependencies:** 1.1
- **Scope:** Define `Permission` enum, `PermissionOverride` entity (UserId, Permission, Granted), and `IPermissionEvaluator` interface. Add default permission matrix per role. Domain tests for permission resolution logic (base role + overrides).
- **TDD:** Unit tests for every combination: agent + override grant, admin + override deny, owner (all permissions).

### Task 1.4 — Permission Persistence & Authorization Policies
- **Status:** DONE
- **Dependencies:** 1.3
- **Scope:** EF Core for PermissionOverride. ASP.NET Core authorization policies that evaluate permissions dynamically (not hardcoded role checks). Middleware/handler to load user permissions. API endpoints: list permissions for user, set override, remove override.
- **TDD:** Integration tests verifying authorization enforcement at API level.

### Task 1.5 — Orgs-API NuGet SDK Client
- **Status:** DONE
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

**Goal:** Build the core domain — property management with full CRUD, lifecycle, media, multilingual descriptions, and JSON Forms. The property model includes fields informed by portal schemas (Kyero, SpainHouses, Thribee) to ensure compatibility if publication is enabled later.

### Task 2.1 — Property Domain Model
- **Status:** PENDING
- **Dependencies:** 0.3
- **Scope:** Define `Property` aggregate root with:
  - Enums: `PropertyType` (Apartment, House, Villa, Penthouse, Studio, Commercial, Land, Garage, StorageRoom, Building, Office), `OperationType` (Sale, Rent, SaleOrRent, Transfer, Vacation), `PropertyStatus` (Draft, Active, Reserved, Sold, Rented, Archived), `EnergyRating` (A-G + Exempt), `Orientation` (N/NE/E/SE/S/SW/W/NW)
  - Value objects: `Address` (street, city, province, postalCode, country, provinceCode (INE 2-digit), municipalityCode (INE 5-digit), latitude, longitude), `PropertyFeatures` (bedrooms, bathrooms, builtArea, usableArea, plotArea, floor, orientation, yearBuilt, energyRating, energyConsumption, energyEmissions, hasPool, hasGarden, hasGarage, hasElevator, hasTerrace, airConditioning, heating, furnished, parkingSpaces), `PropertyFinancials` (price, communityFees, ibiTax, catastroReference), `LocalizedText` (es, pt, en, fr, de, nl)
  - Behavior: `ChangeStatus()` with state machine validation — `Sold`, `Rented`, `Archived` are terminal states
  - Events: `PropertyCreatedV1`, `PropertyUpdatedV1`, `PropertyStatusChangedV1`, `PropertyDeletedV1`
  - Agent assignment: `AgentId` (FK to user), tenant isolation: `TenantId` (FK to org)
  - Calculated: `PricePerSqm` (price / builtArea when both > 0)
  - Soft-delete support via `ISoftDeletable`
- **TDD:** Exhaustive unit tests for state transitions, invariants, validation.

### Task 2.2 — Property Persistence & Repository
- **Status:** PENDING
- **Dependencies:** 2.1
- **Scope:** EF Core DbContext, entity configurations (owned types for value objects), migrations, repository implementations, query specifications for filtering/sorting/pagination.
- **TDD:** Integration tests with Testcontainers PostgreSQL.

### Task 2.3 — Property CRUD API
- **Status:** PENDING
- **Dependencies:** 2.2, 1.5
- **Scope:** CQRS commands and queries: CreateProperty, UpdateProperty, DeleteProperty (soft), ChangePropertyStatus, AssignAgent, ListProperties (paginated, filtered, sorted), GetPropertyById. Authorization via orgs-api SDK (agent sees own, admin/owner sees all, viewer reads only).
- **TDD:** Integration tests for every endpoint + authorization scenarios (agent owns, agent doesn't own, admin, viewer).

### Task 2.4 — Property JSON Schema & Validation
- **Status:** PENDING
- **Dependencies:** 2.1
- **Scope:**
  1. Define JSON Schema for property data entry (all fields from 2.1)
  2. Define UI Schema for layout and conditional visibility
  3. Set up `@jsonforms/react` + `@jsonforms/material-renderers` in the web app
  4. Create custom renderers for: multi-language text, media upload, geo picker, price with currency
  5. i18n integration via JSON Forms `translate` function
- **TDD:** Component tests for custom renderers. Schema validation tests.

### Task 2.5 — Property Media Management
- **Status:** PENDING
- **Dependencies:** 2.2
- **Scope:**
  - `PropertyMedia` entity: id, propertyId, type (Photo, FloorPlan, Document, Video, VirtualTour), url, thumbnailUrl, sortOrder, caption, mimeType, sizeBytes
  - For Photo/FloorPlan/Document: upload to GCP Cloud Storage, generate thumbnails
  - For Video/VirtualTour: store external URL only (YouTube, Matterport)
  - API: upload media, reorder, delete, set primary photo
  - Storage abstraction: `IFileStorageService` with GCP implementation + local filesystem for dev
- **TDD:** Unit tests for media validation. Integration tests for upload/download. Mock storage for unit tests.

### Task 2.6 — Properties-API NuGet SDK Client
- **Status:** PENDING
- **Dependencies:** 2.3, 0.4
- **Scope:** Create `Propely.PropertiesApi.Client` with Refit interfaces: get property by ID, list properties (filtered), get property media. Used by ai-api, contacts-api, appointments-api.
- **TDD:** Contract tests.

### Task 2.7 — Frontend: Property List
- **Status:** PENDING
- **Dependencies:** 2.3
- **Scope:**
  1. Use case definition: list properties (filter by status, type, operation, agent), paginated grid/list views
  2. Stitch MCP design generation
  3. Implementation: property list with filters/pagination
- **TDD:** Component tests. E2E test for list navigation.

### Task 2.8 — Frontend: Property Create & Edit Form
- **Status:** PENDING
- **Dependencies:** 2.3, 2.4
- **Scope:**
  1. Use case definition: create property (JSON Forms wizard), edit property
  2. Stitch MCP design generation
  3. Implementation: multi-step form with JSON Forms, media upload, map picker, preview before save
- **TDD:** Component tests for form steps. Integration test for full creation flow.

### Task 2.9 — Frontend: Property Detail View
- **Status:** PENDING
- **Dependencies:** 2.3, 2.5
- **Scope:**
  1. Use case definition: view property detail with gallery, map, descriptions, status actions
  2. Stitch MCP design generation
  3. Implementation: property detail page with gallery, map, descriptions, status change buttons
- **TDD:** Component tests. E2E test for detail navigation.

### Task 2.10 — Property Search & Advanced Filters
- **Status:** PENDING
- **Dependencies:** 2.3
- **Scope:** Full-text search, advanced filtering (price range, area range, features), saved filters, sorting options. Query specifications for complex filter combinations.
- **TDD:** Integration tests for search scenarios. Component tests for filter UI.

---

## Phase 3 — AI Action Engine

**Goal:** Build the core AI orchestration layer that enables users to perform any system action through natural language (text or voice). This is the platform's primary differentiator.

**Existing base:** The `ParseWorkItemCommand` pattern in ai-api already demonstrates NL text → structured output. Phase 3 extends this to the entire domain with function calling, cross-service SDKs, and voice input.

### Task 3.1 — Intent Classifier & Action Router
- **Status:** DONE
- **Dependencies:** 0.4
- **Scope:** Build the core AI orchestration infrastructure in `ai-api`:
  - `POST /api/actions/execute` — accepts text input, classifies intent via OpenAI function calling, routes to the appropriate action handler, returns structured result + NL confirmation
  - `IActionHandler<TAction, TResult>` interface pattern for typed action dispatch
  - `ActionRouter` service that maps classified intents to handlers via DI
  - Action definitions as OpenAI tool schemas (one per action type)
  - Conversation context support (session-scoped, for follow-up commands)
- **TDD:** Unit tests for router, handler dispatch, intent mapping. Integration test for end-to-end pipeline.

### Task 3.2 — Property Actions via Natural Language
- **Status:** DONE
- **Dependencies:** 3.1, 2.6
- **Scope:** Implement action handlers for property operations:
  - `CreatePropertyAction` — extract fields from NL, create property via PropertiesApi SDK
  - `UpdatePropertyAction` — update specific fields ("change the price to 300k")
  - `QueryPropertyAction` — search/filter ("show me all apartments in Malaga under 200k")
  - `ChangePropertyStatusAction` — "activate", "reserve", "mark as sold"
  - Confidence-scored field extraction with user confirmation for low-confidence fields
- **TDD:** Unit tests for each handler with mocked SDK. Integration tests for NL→action→SDK pipeline.

### Task 3.3 — AI Content Generation
- **Status:** DONE
- **Dependencies:** 3.1
- **Scope:** AI-powered content generation exposed through the action engine:
  - `GenerateCopyAction` — marketing descriptions in multiple languages (es, en, fr, de, nl) with tone options (professional, luxury, casual, concise)
  - `ExtractFromTextAction` — paste property listing text → structured property data with confidence scores
  - `ExtractFromPhotosAction` — upload photos → extract property type, features, room count via vision API
  - Prompt engineering for Spanish real estate terminology
- **TDD:** Unit tests with mocked OpenAI. Prompt regression test suite (20+ cases).

### Task 3.4 — Contact & Lead Actions via Natural Language
- **Status:** BLOCKED
- **Dependencies:** 3.1, 4.5
- **Scope:** Action handlers for contacts and leads:
  - `CreateLeadAction` — "new lead from Maria Garcia for the apartment on Calle Mayor"
  - `CreateContactAction` — "add contact Juan Lopez, buyer, 650123456"
  - `QualifyLeadAction` — "qualify the lead from Maria"
  - `ConvertLeadAction` — "convert Maria's lead to a contact"
  - `QueryLeadsAction` — "show me new leads from this week"
  - Automatic property matching from NL descriptions
- **TDD:** Unit tests for each handler. Integration tests for cross-service action execution.

### Task 3.5 — Operation Actions via Natural Language
- **Status:** DONE
- **Dependencies:** 3.2
- **Scope:** Action handlers for property lifecycle operations:
  - `ReservePropertyAction` — "reserve the apartment on Calle Mayor for Maria Garcia"
  - `CloseOperationAction` — "close the sale of AP-2024-001" (marks sold/rented based on operation type)
  - `ArchivePropertyAction` — "archive property AP-2024-015"
  - `ReactivatePropertyAction` — "relist the apartment on Calle Mayor"
  - Operations require entity resolution: match property references (code, address, or description) to actual properties
- **TDD:** Unit tests for operation handlers. Entity resolution tests with fuzzy matching.

### Task 3.6 — Appointment Actions via Natural Language
- **Status:** BLOCKED
- **Dependencies:** 3.1, 5.8
- **Scope:** Action handlers for appointments:
  - `BookViewingAction` — "book a viewing for the Malaga villa with Maria Garcia next Tuesday at 10am"
  - `QueryAppointmentsAction` — "what do I have scheduled this week?"
  - `CancelAppointmentAction` — "cancel Tuesday's viewing"
  - `RescheduleAppointmentAction` — "move the viewing to Wednesday at 3pm"
  - Relative date parsing ("next Tuesday", "this Friday", "tomorrow afternoon")
- **TDD:** Unit tests for each handler. Date parsing tests with Spanish locale.

### Task 3.7 — Voice Input (Speech-to-Text)
- **Status:** DONE
- **Dependencies:** 3.1
- **Scope:** Voice transcription endpoint in `ai-api`:
  - `POST /api/voice/transcribe` — accepts audio (WebM/Opus from browser MediaRecorder, WAV, MP3)
  - Uses `gpt-4o-mini-transcription` via OpenAI Audio API
  - Returns transcribed text + language detected
  - After transcription, optionally chains into `POST /api/actions/execute` for a single-step voice→action pipeline
  - `POST /api/voice/execute` — accepts audio, transcribes, executes action, returns result (combined endpoint)
  - Audio size limit: 25 MB (OpenAI limit)
  - Supported languages: Spanish (primary), English, French, German, Dutch
- **TDD:** Unit tests with mocked audio API. Integration test for voice→text→action pipeline.

### Task 3.8 — AI-API NuGet SDK Client
- **Status:** DONE
- **Dependencies:** 3.1, 3.7
- **Scope:** Create `Propely.AiApi.Client` NuGet package:
  - `IActionApi` — `ExecuteActionAsync(text)`, `ExecuteVoiceActionAsync(audio)`
  - `IPropertyExtractionApi` — `ExtractFromTextAsync`, `ExtractFromPhotosAsync`
  - `IContentGenerationApi` — `GenerateCopyAsync`
  - `IVoiceApi` — `TranscribeAsync(audio)`
  - Typed request/response DTOs, Polly policies (60s timeout for AI calls)
- **TDD:** DI registration tests. Polly policy tests.

### Task 3.9 — Prompt Engineering & Spanish RE Vocabulary
- **Status:** BLOCKED
- **Dependencies:** 3.2, 3.3, 3.4
- **Scope:** Systematic prompt tuning for the Spanish real estate domain:
  - Property type vocabulary: piso, adosado, chalet, atico, bajo, duplex, finca, cortijo, local, oficina, nave, solar, garaje
  - Spanish-specific fields: catastro, IBI, comunidad de propietarios, certificado energetico
  - Few-shot examples for common Spanish listing formats (Idealista, Fotocasa style)
  - Prompt regression test suite: 20+ input/expected-output pairs
  - Bilingual support (mixed Spanish/English input)
- **TDD:** Regression tests run as part of `dotnet test`. Accuracy threshold >= 85%.

### Task 3.10 — Frontend: Command Bar
- **Status:** DONE
- **Dependencies:** 3.8
- **Scope:**
  1. Use case definition: global command bar for text input, accessible via `Ctrl+K` / `Cmd+K`
  2. Stitch MCP design generation
  3. Implementation:
     - `cmdk`-based command palette overlay
     - Free-form text input at the top
     - Action suggestions as user types (debounced)
     - Result display: confirmation, created entity link, error with retry
     - Recent commands history (session-scoped)
     - Keyboard navigation
- **TDD:** Component tests for command bar interactions. E2E test for text→action→result flow.

### Task 3.11 — Frontend: Voice Mode
- **Status:** DONE
- **Dependencies:** 3.10, 3.7
- **Scope:**
  1. Use case definition: microphone button in command bar, hold-to-record or toggle
  2. Stitch MCP design generation
  3. Implementation:
     - Microphone button in command bar with recording indicator (pulsing ring)
     - Browser `MediaRecorder` API for audio capture (WebM/Opus or WAV)
     - `useVoiceInput` hook handling: permission request, recording start/stop, audio upload
     - Real-time waveform visualization during recording
     - Transcribed text appears in command bar input, then auto-executes
     - Error handling: microphone permission denied, no audio detected, transcription failure
     - Mobile-friendly: large tap target, haptic feedback (vibration API)
- **TDD:** Component tests with mocked MediaRecorder. Hook tests for recording lifecycle.

---

## Phase 4 — Contacts & Leads

**Goal:** Build the contacts domain with dual-role contacts, lead management, and property linkage. NL actions for contacts/leads are handled in P3.4.

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
- **Scope:** `Propely.ContactsApi.Client` with Refit interfaces: get contact, list contacts, get leads for property, create lead. Used by ai-api for NL lead actions.
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

**Goal:** Enable scheduling of property viewings, meetings, and generic appointments with bidirectional calendar sync. NL scheduling is handled in P3.6.

### Task 5.1 — Appointment Domain Model
- **Status:** DONE
- **Dependencies:** 0.3
- **Scope:** Define `Appointment` aggregate root:
  - Types: `AppointmentType` (PropertyViewing, OwnerMeeting, Generic)
  - Fields: type, title, description, startUtc, endUtc, location, propertyId (nullable — required for viewings), contactId (nullable), agentId (required), status (Scheduled, Confirmed, Completed, Cancelled, NoShow), notes, externalCalendarEventId
  - Behavior: `Confirm()`, `Complete(notes)`, `Cancel(reason)`, `MarkNoShow()`
  - Events: `AppointmentScheduledV1`, `AppointmentStatusChangedV1`, `AppointmentCancelledV1`
  - Tenant isolation: `BranchId`
- **TDD:** Unit tests for appointment lifecycle, validation (end > start, viewing requires property).

### Task 5.2 — Appointments Persistence & API
- **Status:** DONE
- **Dependencies:** 5.1
- **Scope:** EF Core setup for appointments-api. CRUD endpoints. List appointments by agent/date range/property/contact. Authorization: agent sees own, admin sees all.
- **TDD:** Integration tests for all endpoints + authorization.

### Task 5.3 — Google Calendar Integration
- **Status:** DONE
- **Dependencies:** 5.2
- **Scope:** OAuth2 flow for Google Calendar. Bidirectional sync: create/update/delete Propely appointment → push to Google Calendar. Google Calendar webhook for external changes → update Propely. Store sync state (externalCalendarEventId, lastSyncedAt). Handle conflicts.
- **TDD:** Unit tests with mocked Google API. Integration tests for sync scenarios.

### Task 5.4 — Outlook/Microsoft Graph Integration
- **Status:** DONE
- **Dependencies:** 5.2
- **Scope:** OAuth2 flow for Microsoft Graph. Same bidirectional sync pattern as Google Calendar. `ICalendarSyncService` interface with Google and Microsoft implementations.
- **TDD:** Unit tests with mocked Graph API.

### Task 5.5 — Calendar Sync Engine
- **Status:** DONE
- **Dependencies:** 5.3, 5.4
- **Scope:** Background service that processes sync queue. Handles rate limiting, retry with backoff, conflict resolution (last-write-wins with user notification). Publishes `AppointmentSyncedV1` events.
- **TDD:** Unit tests for conflict resolution. Integration tests for sync queue processing.

### Task 5.6 — Frontend: Calendar View
- **Status:** DONE
- **Dependencies:** 5.2, 1.6
- **Scope:**
  1. Use case definition: view calendar (month/week/day), create appointment from calendar, drag to reschedule, click for details
  2. Stitch MCP design generation
  3. Implementation with FullCalendar: month/week/day views, event coloring by type, drag-and-drop reschedule, appointment detail popover
- **TDD:** Component tests for calendar wrapper. E2E test for create/view/reschedule flow.

### Task 5.7 — Frontend: Appointment Booking
- **Status:** DONE
- **Dependencies:** 5.6
- **Scope:**
  1. Use case definition: book viewing from property detail page, book meeting from contact detail, agent availability check
  2. Stitch MCP design generation
  3. Implementation: booking modal with date/time picker, contact selection, property selection (for viewings)
- **TDD:** Component tests. Integration test for booking flow.

### Task 5.8 — Appointments-API NuGet SDK Client
- **Status:** DONE
- **Dependencies:** 5.2
- **Scope:** `Propely.AppointmentsApi.Client` with Refit interfaces: create appointment, list by agent/date, get by ID. Used by ai-api for NL appointment actions.
- **TDD:** Contract tests.

---

## Phase 6 — Portal Reference (Deprioritized)

**Goal:** Portal publication infrastructure is **not actively developed**. The `publishing-api` service is scaffolded but has no active tasks. Portal schemas (Kyero v3.9, SpainHouses XSD, Thribee) are used as **reference documentation** for property model fields.

**Status:** DEPRIORITIZED — All original tasks moved to backlog. Lead intake webhooks may be recovered when there is business demand.

### What is kept:
- `services/publishing-api/` scaffolding (Clean Architecture shell) — exists, no changes planned
- Portal schema documentation as reference for property field definitions

### What is deferred:
- Publication domain model and status machine (old 6.1)
- Portal adapters: Kyero, Thribee, SpainHouses (old 6.2–6.4)
- Publishing worker (old 6.5)
- Lead intake webhooks (old 6.6) — recoverable from backlog
- Frontend publication management (old 6.7)

---

## Phase 7 — Intelligence & Analytics

**Goal:** Dashboard, analytics, AI conversation intelligence, and platform polish.

### Task 7.1 — Dashboard & Analytics
- **Status:** PENDING
- **Dependencies:** 2.3, 4.3, 5.2
- **Scope:**
  1. Use case definition: agent dashboard (my properties by status, my leads, upcoming appointments), admin dashboard (branch metrics, agent performance)
  2. Stitch MCP design generation
  3. Implementation: dashboard pages with charts and KPIs
- **TDD:** Component tests. API tests for analytics endpoints.

### Task 7.2 — Conversation Context & Memory
- **Status:** PENDING
- **Dependencies:** 3.1
- **Scope:** Session-scoped conversation context for the AI action engine:
  - Remember entities referenced in previous commands ("the apartment" → last mentioned property)
  - Pronoun resolution ("reserve it for her" → last property + last contact)
  - Conversation history display in command bar
  - Context window management (last N exchanges)
  - Server-side session storage with TTL
- **TDD:** Unit tests for context resolution. Integration tests for multi-turn conversations.

### Task 7.3 — Proactive AI Suggestions
- **Status:** PENDING
- **Dependencies:** 3.1, 4.3
- **Scope:** AI-generated suggestions based on data patterns:
  - "You have 3 leads without contact in 7 days"
  - "Property AP-2024-001 has been in Draft for 14 days"
  - "Maria Garcia has asked about 3 similar properties — consider a grouped viewing"
  - Suggestions displayed as a notification feed and optionally in the command bar
  - Rule engine: configurable trigger conditions + AI-generated human-readable text
- **TDD:** Unit tests for suggestion rules. Integration tests for end-to-end suggestion pipeline.

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
Phase 2 ──┼── 2.1 Property ─── 2.2 Persistence ── 2.3 CRUD API ── 2.6 SDK
           │   └── 2.4 Schema   └── 2.5 Media     │  └── 2.10 Search
           │                                       └── 2.7 List UI
           │                                       └── 2.8 Create UI (needs 2.4)
           │                                       └── 2.9 Detail UI (needs 2.5)
           │
Phase 3 ──┼── 3.1 Intent Classifier ─── 3.2 Property Actions
  (AI)     │   │                         3.3 Content Generation
           │   ├── 3.4 Contact/Lead Actions (needs 4.5)
           │   ├── 3.5 Operation Actions
           │   ├── 3.6 Appointment Actions (needs 5.8)
           │   ├── 3.7 Voice Input (STT)
           │   ├── 3.8 AI SDK Client
           │   ├── 3.9 Prompt Engineering
           │   ├── 3.10 Frontend: Command Bar
           │   └── 3.11 Frontend: Voice Mode
           │
Phase 4 ──┼── 4.1 Contact ──── 4.2 Lead ──── 4.3 API ──── 4.5 SDK
           │                                  └── 4.4 Conversion
           │                                  └── 4.6 Contact UI ── 4.7 Lead UI
           │
Phase 5 ──┼── 5.1 Appointment ── 5.2 API ──── 5.3 Google ─┐
           │                                   5.4 Outlook ─┤── 5.5 Sync
           │                                   └── 5.6 Calendar UI ── 5.7 Booking
           │                                   └── 5.8 SDK
           │
Phase 6 ──── (DEPRIORITIZED — scaffolded only, portal schemas as reference)
           │
Phase 7 ──┴── 7.1 Dashboard ── 7.2 Conversation Context
           │                    7.3 Proactive Suggestions
           │                    7.4 Design Refinement
           │
Phase 8 ──┼── 8.5 Shared TenantDelegatingHandler (independent)
  (AI      │
  Provider ├── 8.1 Tool Schema Registry
  Abstrac- │   ├── 8.2 Typed Parameters ── 8.3 Classifier Adapter
  tion)    │   └── 8.4 MCP Server Endpoint
```

### Recommended execution order

The AI action engine (P3) depends on service SDKs being available. The recommended build order interleaves domain and AI work:

1. **P1 frontend** (1.6, 1.7) — unblock UI development
2. **P2 core** (2.1–2.6) — property domain + persistence + API + media + SDK
3. **P3 core** (3.1, 3.2, 3.3, 3.7) — intent classifier, property NL actions, voice STT
4. **P2 frontend** (2.4, 2.7–2.10) — JSON schema, property UI (list, create, detail, search)
5. **P4 core** (4.1–4.5) — contacts/leads domain + SDK
6. **P3 extend** (3.4, 3.5) — contact/lead NL actions, operation actions
7. **P5 core** (5.1–5.2, 5.8) — appointments domain + SDK
8. **P3 extend** (3.6) — appointment NL actions
9. **P3 frontend** (3.8–3.11) — SDK client, prompt tuning, command bar, voice
10. **P4–P5 frontend** — contacts UI, calendar UI
11. **P5 sync** (5.3–5.5) — calendar integrations
12. **P7** — dashboard, conversation context, proactive suggestions
13. **P8** — AI provider abstraction, MCP server, typed parameters, shared infra

---

## Phase 8 — AI Provider Abstraction & MCP Server

**Goal:** Decouple the AI Action Engine from OpenAI-specific types, add typed action parameters, expose ai-api as an MCP server for multi-LLM interoperability, and deduplicate shared infrastructure.

**Dependencies:** All of P3 (DONE). Independent of P7.

### Task 8.1 — Provider-Neutral Tool Schema Registry
- **Status:** DONE
- **Dependencies:** 3.1 (DONE)
- **Scope:** Extract 20 tool definitions from OpenAI-coupled `ToolDefinitions.cs` into provider-neutral `ToolSchemaRegistry` (JSON Schema records). Create `IToolAdapter` interface with `OpenAiToolAdapter` implementation. Delete `ToolDefinitions.cs`.
- **TDD:** Unit tests for all 20 schemas, ActionType resolution, adapter output equivalence.

### Task 8.2 — Typed Action Parameter Records
- **Status:** PENDING
- **Dependencies:** 8.1
- **Scope:** Replace `Dictionary<string,object?>` + `ParameterExtractor` with 20 typed parameter records (one per action). Create `IParameterBinder` for type-safe conversion. Refactor all 19 action handlers to use typed properties. Delete `ParameterExtractor.cs`.
- **TDD:** Binder type conversion tests, handler refactor with behavioral equivalence.

### Task 8.3 — OpenAI Classifier Adapter Refactor
- **Status:** PENDING
- **Dependencies:** 8.1, 8.2
- **Scope:** Refactor `OpenAiIntentClassifier` to use `IToolSchemaRegistry` + `OpenAiToolAdapter` via DI injection. Add keyed DI for multi-provider support preparation. No OpenAI types in Application layer.
- **TDD:** Adapter injection tests, provider DI resolution tests.

### Task 8.4 — MCP Server Endpoint
- **Status:** PENDING
- **Dependencies:** 8.1
- **Scope:** Add `ModelContextProtocol` NuGet package to ai-api. Mount MCP Streamable HTTP endpoint at `/mcp`. Register 20 tools from `IToolSchemaRegistry`. Route `tools/call` through `ActionRouter`. Enforce auth + tenant isolation.
- **TDD:** Integration tests for tools/list, tools/call, auth enforcement, behavioral equivalence with REST.

### Task 8.5 — Shared TenantDelegatingHandler Package
- **Status:** PENDING
- **Dependencies:** None
- **Scope:** Create `Propely.Shared.Http` package with single `TenantDelegatingHandler`. Update all 5 SDK clients to reference shared package. Delete 5 duplicate files.
- **TDD:** Existing tests verify behavioral equivalence.

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
| OpenAI (NuGet) | latest | Chat completions, function calling, audio API | MIT |
| ModelContextProtocol | latest | .NET MCP SDK for AI tool interoperability | MIT |
| cmdk | latest | Command palette/bar for NL input | MIT |
