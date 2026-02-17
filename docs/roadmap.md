# Propely — Roadmap

## Vision

Propely is a **multi-tenant, AI-powered real estate management SaaS**. The property is the central entity around which all features revolve: form-based data entry (with AI smart-fill), publication to real estate portals, lead capture, and appointment scheduling with calendar sync.

---

## Phase 0 — Foundation & Documentation Overhaul

| # | Task | Status |
|---|------|--------|
| 0.1 | Full documentation & agent-instructions overhaul | DONE |
| 0.2 | Define NuGet SDK convention & scaffold first SDK (`Propely.AiApi.Client`) | PENDING |
| 0.3 | CI pipeline for NuGet packages | PENDING |
| 0.4 | Tenant context propagation convention | PENDING |
| 0.5 | Port allocation & docker-compose for new services | PENDING |

---

## Phase 1 — Properties Microservice

| # | Task | Status | Dependencies |
|---|------|--------|-------------|
| 1.1 | Domain model: `Property` aggregate root | PENDING | 0.5 |
| 1.2 | Application layer: CRUD commands + queries | PENDING | 1.1 |
| 1.3 | Infrastructure: EF Core + migrations | PENDING | 1.1 |
| 1.4 | API endpoints | PENDING | 1.2, 1.3 |
| 1.5 | Media management | PENDING | 1.4 |
| 1.6 | Domain events (outbox) | PENDING | 1.4 |
| 1.7 | `Propely.PropertiesApi.Client` NuGet SDK | PENDING | 1.4, 0.2 |
| 1.8 | Property form (JSON Forms + JSON Schema) | PENDING | 1.4 |
| 1.9 | Multi-language support | PENDING | 1.8 |
| 1.10 | Multi-tenant schema management | PENDING | 1.8 |

---

## Phase 2 — AI Smart-Fill & Form Enhancement

| # | Task | Status | Dependencies |
|---|------|--------|-------------|
| 2.1 | AI API: extract property fields endpoint (gpt-5-mini) | PENDING | 1.4, 0.2 |
| 2.2 | Prompt engineering & optimization | PENDING | 2.1 |
| 2.3 | `Propely.AiApi.Client` SDK enhancement | PENDING | 2.1 |
| 2.4 | Properties API: smart-fill orchestration | PENDING | 2.3, 1.7 |
| 2.5 | Frontend: smart-fill UX | PENDING | 2.4, 1.8 |
| 2.6 | AI copy generation (optional) | PENDING | 2.4 |

---

## Phase 3 — Publication & Contacts

| # | Task | Status | Dependencies |
|---|------|--------|-------------|
| 3.1 | Contacts microservice | PENDING | 0.5 |
| 3.2 | `Propely.ContactsApi.Client` NuGet SDK | PENDING | 3.1, 0.2 |
| 3.3 | Publishing microservice: domain model | PENDING | 0.5 |
| 3.4 | Portal adapter: Kyero | PENDING | 3.3, 1.7 |
| 3.5 | Portal adapter: Thribee | PENDING | 3.3, 1.7 |
| 3.6 | Portal adapter: SpainHouses | PENDING | 3.3, 1.7 |
| 3.7 | Publishing worker | PENDING | 3.4, 3.5, 3.6 |
| 3.8 | Lead intake webhook | PENDING | 3.2, 3.7 |
| 3.9 | Contract tests | PENDING | 3.4, 3.5, 3.6 |
| 3.10 | Frontend: publish UI | PENDING | 3.7 |

---

## Phase 4 — Appointments & Calendar

| # | Task | Status | Dependencies |
|---|------|--------|-------------|
| 4.1 | Appointments microservice: domain model | PENDING | 0.5 |
| 4.2 | CRUD + query endpoints | PENDING | 4.1 |
| 4.3 | Google Calendar integration (bidirectional) | PENDING | 4.2 |
| 4.4 | Outlook/Graph integration (bidirectional) | PENDING | 4.2 |
| 4.5 | Sync engine (conflict resolution, renewal) | PENDING | 4.3, 4.4 |
| 4.6 | Frontend: calendar view (FullCalendar) | PENDING | 4.2 |
| 4.7 | Frontend: appointment booking | PENDING | 4.6 |

---

## Phase 5 — Polish & Extensions

| # | Task | Status | Dependencies |
|---|------|--------|-------------|
| 5.1 | Additional portal adapters (Green-Acres, Idealista) | PENDING | 3.7 |
| 5.2 | AI-powered property valuation | PENDING | 2.4 |
| 5.3 | Dashboard & analytics | PENDING | 3.10, 4.7 |
| 5.4 | Google Stitch UI designs (all screens) | PENDING | — |

---

## Key Library Decisions

| Area | Library | License |
|------|---------|---------|
| Property Form | JSON Forms (`@jsonforms/react`) | MIT |
| Calendar UI | FullCalendar Standard | MIT |
| NuGet SDK Clients | Refit | MIT |
| AI Model | gpt-5-mini | OpenAI API |

## Portal Targets

| Portal | Format | Coverage |
|--------|--------|----------|
| Kyero | XML v3.9 | Spain/Portugal international buyers |
| Thribee | XML | Trovit, Mitula, Nestoria, Nuroa |
| SpainHouses.net | XML + XSD | Spain domestic |
