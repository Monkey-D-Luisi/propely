# Task: 0050-dashboard-analytics

## Metadata
- ID: 0050
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-08
- Related docs:
  - Walkthrough: `docs/walkthroughs/0050-dashboard-analytics.md`
  - Epic: `docs/backlog/epic-P7-polish.md` (Task 7.1)

## Goal
Build a KPI dashboard with charts and quick actions, powered by count-by-status endpoints in each downstream service, aggregated client-side.

## Context
The platform had no dashboard — the root page was empty. Agents and admins had no way to see portfolio health at a glance (active listings, open leads, upcoming appointments, conversions). The epic originally specified orgs-api as the aggregation layer, but orgs-api uses `GetUserId()` not `GetTenantId()`, has zero downstream SDK client references, and adding tenant propagation would require significant plumbing. A frontend-direct aggregation approach was chosen instead.

## Scope
### In scope
- Count-by-status endpoint on contacts-api (`GET /api/leads/count-by-status`)
- Count-by-status + count-upcoming endpoints on appointments-api (`GET /api/appointments/count-by-status`, `GET /api/appointments/count-upcoming`)
- `use-dashboard` hook fetching from 3 services in parallel
- KPI grid with 4 cards: Active Listings, Open Leads, Upcoming Appointments, Converted This Month
- StatusPieChart (properties by status, recharts donut)
- StatusBarChart (leads by status, recharts horizontal bar)
- Quick actions: New Property, New Lead, Schedule Appointment
- Dashboard nav item in sidebar
- i18n (en, es)
- Component tests (14 tests across 3 files)

### Out of scope
- Date range filtering (future enhancement)
- Agent vs admin differentiated dashboards (v2)
- Activity feed / recent history
- Server-side aggregation in orgs-api
- Redis caching layer

## Requirements
- R1: contacts-api exposes `GET /api/leads/count-by-status` returning `Dictionary<string, int>`
- R2: appointments-api exposes `GET /api/appointments/count-by-status` returning `Dictionary<string, int>`
- R3: appointments-api exposes `GET /api/appointments/count-upcoming?days=7` returning `{ count: int }`
- R4: Dashboard page renders KPI cards with derived values (active listings, open leads = New+Contacted+Qualified, upcoming, converted)
- R5: Charts render property status as pie chart and lead status as bar chart
- R6: Quick actions link to new property, new lead, schedule appointment
- R7: Loading skeleton shown while data is fetching
- R8: Null data handled gracefully (zero values)

## Changes

### New backend files

#### contacts-api
| File | Purpose |
|------|---------|
| `Application/Leads/Queries/CountLeadsByStatus/CountLeadsByStatusQuery.cs` | CQRS query + handler, converts enum keys to string |
| `UnitTests/Application/Leads/Queries/CountLeadsByStatusQueryHandlerTests.cs` | 3 tests: returns counts, empty results, passes tenant ID |

#### appointments-api
| File | Purpose |
|------|---------|
| `Application/Appointments/Queries/CountAppointmentsByStatus/CountAppointmentsByStatusQuery.cs` | CQRS query + handler |
| `Application/Appointments/Queries/CountUpcomingAppointments/CountUpcomingAppointmentsQuery.cs` | CQRS query + handler (default days=7) |
| `Client/Models/CountUpcomingResponse.cs` | Response DTO for count-upcoming endpoint |
| `UnitTests/.../CountAppointmentsByStatusQueryHandlerTests.cs` | 3 tests |
| `UnitTests/.../CountUpcomingAppointmentsQueryHandlerTests.cs` | 3 tests |

### Modified backend files
| File | Change |
|------|--------|
| `contacts-api/.../ILeadReadRepository.cs` | Added `CountByStatusAsync` |
| `contacts-api/.../LeadReadRepository.cs` | GroupBy implementation |
| `contacts-api/.../LeadsController.cs` | Added count-by-status endpoint |
| `contacts-api/.../ILeadsApiClient.cs` | Added Refit client method |
| `appointments-api/.../IAppointmentReadRepository.cs` | Added `CountByStatusAsync` + `CountUpcomingAsync` |
| `appointments-api/.../AppointmentReadRepository.cs` | GroupBy + date filter implementations |
| `appointments-api/.../AppointmentsController.cs` | Added 2 endpoints |
| `appointments-api/.../IAppointmentsApiClient.cs` | Added 2 client methods |

### New frontend files
| File | Purpose |
|------|---------|
| `hooks/use-dashboard.ts` | Fetches from 3 services in parallel, returns aggregated DashboardData |
| `components/dashboard/KpiGrid.tsx` | KpiCard + KpiGrid components (4 KPI cards) |
| `components/dashboard/charts/StatusPieChart.tsx` | Recharts donut PieChart for properties |
| `components/dashboard/charts/StatusBarChart.tsx` | Recharts horizontal BarChart for leads |
| `components/dashboard/QuickActions.tsx` | 3 shortcut action links |
| `app/[locale]/(dashboard)/page.tsx` | Dashboard page with loading skeleton |
| `components/dashboard/__tests__/KpiGrid.test.tsx` | 4 component tests |
| `components/dashboard/__tests__/QuickActions.test.tsx` | 3 component tests |
| `components/dashboard/__tests__/DashboardPage.test.tsx` | 7 page-level tests |

### Modified frontend files
| File | Change |
|------|--------|
| `components/layout/AppSidebar.tsx` | Added Dashboard nav item with exact path matching |
| `messages/en.json` | Added `dashboard` namespace (15 keys) |
| `messages/es.json` | Added `dashboard` namespace (15 keys) |
| `package.json` | Added recharts dependency |

## Verification
```bash
dotnet test services/contacts-api/Propely.ContactsApi.sln    # 279 tests pass
dotnet test services/appointments-api/Propely.AppointmentsApi.sln  # 300 tests pass
cd apps/web && npm test                                       # 838 tests pass
```
