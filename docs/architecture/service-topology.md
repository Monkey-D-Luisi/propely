# Service Topology

Propely is a multi-tenant, AI-powered real estate management SaaS built as a microservices monorepo. This document describes the service topology, responsibilities, and data ownership.

## Overview

```
                        +-----------+
                        |  Browser  |
                        +-----+-----+
                              |
                        +-----v-----+
                        |  web      |
                        |  (Next.js)|
                        |  :3000    |
                        +-----+-----+
                              |
         +--------------------+--------------------+
         |          |         |         |           |
   +-----v----+ +--v------+ +v-------+ +v--------+ +v-----------+
   | orgs-api | | ai-api  | |props-  | |contacts-| |appointments|
   | :5020    | | :5010   | |api     | |api      | |-api        |
   |          | |         | |:5030   | |:5050    | |:5060       |
   +-----+----+ +----+----+ +---+----+ +----+----+ +-----+------+
         |           |          |           |             |
         |           |     +----v----+      |             |
         |           |     |publish- |      |             |
         |           |     |ing-api  |      |             |
         |           |     |:5040    |      |             |
         |           |     +---------+      |             |
         |           |          |           |             |
   +-----v-----------v----------v-----------v-------------v------+
   |                    Infrastructure                           |
   |  PostgreSQL :5432 | RabbitMQ :5672 | Redis :6379            |
   +---------------------------------------------------------+
```

## Services

### web (Next.js 16)
- **Port:** 3000
- **Purpose:** Frontend SPA for all user interactions
- **Tech:** Next.js 16, React 19, Tailwind CSS v4, next-intl
- **Key libraries:** JSON Forms (property data entry), FullCalendar (appointments)
- **Data:** No database -- communicates with backend APIs via REST

### orgs-api (.NET 10)
- **Port:** 5020
- **Database:** `propely_orgsapi`
- **Purpose:** Authentication, authorization, agency/branch hierarchy, billing, team management
- **Domains:** Users, Agencies, Organizations (branches), Memberships, Invitations, Permissions, Billing
- **Key role:** Central identity and authorization service consumed by all other backends
- **Published SDK:** `Propely.OrgsApi.Client` -- used by all other .NET services

### ai-api (.NET 10)
- **Port:** 5010
- **Database:** `propely_aiapi`
- **Purpose:** AI capabilities -- field extraction from text/photos, multilingual copy generation
- **Domains:** Property field extraction, content generation, AI prompt management
- **External:** OpenAI API (gpt-5-mini)
- **Published SDK:** `Propely.AiApi.Client` -- consumed by properties-api

### properties-api (.NET 10)
- **Port:** 5030
- **Database:** `propely_propertiesapi`
- **Purpose:** Core property domain -- CRUD, lifecycle, media, multilingual descriptions
- **Domains:** Properties, Media, Descriptions, Property lifecycle (Draft/Active/Reserved/Sold/Rented/Archived)
- **External:** GCP Cloud Storage (media uploads)
- **Published SDK:** `Propely.PropertiesApi.Client` -- consumed by publishing-api, contacts-api, appointments-api

### publishing-api (.NET 10)
- **Port:** 5040
- **Database:** `propely_publishingapi`
- **Purpose:** Portal syndication -- publish listings to Kyero, Thribee, SpainHouses
- **Domains:** Publications, Portal adapters, XML feeds, Lead intake webhooks
- **Consumes:** `Propely.PropertiesApi.Client`, `Propely.ContactsApi.Client`

### contacts-api (.NET 10)
- **Port:** 5050
- **Database:** `propely_contactsapi`
- **Purpose:** Contact and lead management
- **Domains:** Contacts (multi-role: Buyer/Seller/Tenant/Landlord), Leads (property-linked), Lead conversion
- **Published SDK:** `Propely.ContactsApi.Client` -- consumed by appointments-api
- **Consumes:** `Propely.PropertiesApi.Client`

### appointments-api (.NET 10)
- **Port:** 5060
- **Database:** `propely_appointmentsapi`
- **Purpose:** Scheduling -- viewings, meetings, calendar sync
- **Domains:** Appointments, Calendar sync (Google Calendar, Microsoft Outlook)
- **External:** Google Calendar API, Microsoft Graph API
- **Consumes:** `Propely.PropertiesApi.Client`, `Propely.ContactsApi.Client`

## Infrastructure Services

| Service | Port(s) | Purpose |
|---------|---------|---------|
| PostgreSQL 16 | 5432 | Primary datastore (one instance, separate databases per service) |
| RabbitMQ 3 | 5672 / 15672 | Async messaging (domain events, outbox pattern) |
| Redis 7 | 6379 | Caching (read model projections, session data) |
| Aspire Dashboard | 18888 / 4317 / 4318 | OpenTelemetry collector and traces/metrics/logs viewer |
| MailHog | 10025 / 18025 | Dev email capture (SMTP sink) |

## Database Isolation

Each service owns its database. No cross-database queries are allowed. Services access other services' data exclusively through their published SDK clients.

| Service | Database |
|---------|----------|
| orgs-api | `propely_orgsapi` |
| ai-api | `propely_aiapi` |
| properties-api | `propely_propertiesapi` |
| publishing-api | `propely_publishingapi` |
| contacts-api | `propely_contactsapi` |
| appointments-api | `propely_appointmentsapi` |

## Multi-Tenancy

All services enforce tenant isolation at the data level:

- **Tenant = Branch (Organization):** Each branch operates as an isolated tenant
- **Tenant propagation:** The `X-Tenant-Id` header carries the tenant context through all inter-service calls via `TenantDelegatingHandler`
- **Data isolation:** EF Core global query filters automatically scope all queries to the current tenant
- **Agency cross-branch visibility:** Agency owners can access data across their branches through explicit authorization checks

## Clean Architecture (per service)

Every .NET service follows the same layered architecture:

```
src/
  Propely.<Service>.Domain/          Pure business logic, entities, events
  Propely.<Service>.Application/     Use cases (CQRS via MediatR), interfaces
  Propely.<Service>.Infrastructure/  EF Core, RabbitMQ, Redis, external services
  Propely.<Service>.Api/             Controllers, middleware, configuration
  Propely.<Service>.Client/          NuGet SDK client (Refit) for consumers
tests/
  Propely.<Service>.UnitTests/       Domain + Application tests
  Propely.<Service>.IntegrationTests/ API + Infrastructure tests
```

**Dependency rule:** Dependencies flow inward only. Domain has zero framework dependencies. Infrastructure implements interfaces defined in Application.
