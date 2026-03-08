# Walkthrough: 0050-dashboard-analytics

## Overview
Added KPI dashboard with charts and quick actions to the Propely frontend. Backend count-by-status endpoints were added to contacts-api and appointments-api (properties-api already had one). The frontend aggregates data from all 3 services via `use-dashboard` hook.

## Architecture Decision
The epic originally specified orgs-api as the aggregation layer. We chose frontend-direct aggregation instead because:
1. orgs-api uses `GetUserId()` not `GetTenantId()` — tenant propagation to SDK clients would need custom plumbing
2. orgs-api Infrastructure has zero downstream SDK client references
3. The frontend already has service-specific fetchers that auto-include auth + org context

## Phase A: Backend Endpoints

### contacts-api
Added `GET /api/leads/count-by-status` endpoint following the exact pattern from properties-api:
- `ILeadReadRepository.CountByStatusAsync` → GroupBy on Status enum
- `CountLeadsByStatusQuery` + handler converts enum keys to string dict
- `LeadsController.CountByStatus` with `RequireViewer` policy
- `ILeadsApiClient.CountByStatusAsync` Refit method

### appointments-api
Added two endpoints:
1. `GET /api/appointments/count-by-status` — same GroupBy pattern
2. `GET /api/appointments/count-upcoming?days=7` — filters by date range (now to now+days) for Scheduled/Confirmed appointments, returns `{ count: N }`

Both have corresponding CQRS queries, repository methods, controller endpoints, SDK client methods, and unit tests.

## Phase B: Frontend Dashboard

### Data Flow
```
use-dashboard.ts
  ├── useFetch('/api/properties/count-by-status', propertiesApiFetch)
  ├── useFetch('/api/leads/count-by-status', contactsApiFetch)
  ├── useFetch('/api/appointments/count-by-status', appointmentsApiFetch)
  └── useFetch('/api/appointments/count-upcoming?days=7', appointmentsApiFetch)
         ↓
  returns { properties, leads, appointments, isLoading, error }
```

### Dashboard Page Layout
```
┌──────────────────────────────────────────────┐
│ Dashboard                                     │
│ Welcome back, {firstName}!                    │
├──────────────────────────────────────────────┤
│ ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐            │
│ │ KPI │ │ KPI │ │ KPI │ │ KPI │   KpiGrid   │
│ │  1  │ │  2  │ │  3  │ │  4  │            │
│ └─────┘ └─────┘ └─────┘ └─────┘            │
├──────────────────────────────────────────────┤
│ ┌────────────────┐ ┌────────────────┐        │
│ │   PieChart     │ │   BarChart     │ Charts │
│ │  (properties)  │ │   (leads)      │        │
│ └────────────────┘ └────────────────┘        │
├──────────────────────────────────────────────┤
│ [+ New Property] [+ New Lead] [+ Schedule]   │
│                               Quick Actions   │
└──────────────────────────────────────────────┘
```

### KPI Derivations
- Active Listings = `properties.byStatus.Active ?? 0`
- Open Leads = `(leads.New ?? 0) + (leads.Contacted ?? 0) + (leads.Qualified ?? 0)`
- Upcoming Appointments = `appointments.upcoming ?? 0`
- Converted This Month = `leads.Converted ?? 0`

### Charts
- **StatusPieChart**: Recharts donut chart with custom colors per property status (Draft=slate, Active=emerald, Reserved=amber, Sold=sky, Withdrawn=rose)
- **StatusBarChart**: Recharts horizontal BarChart with custom colors per lead status (New=sky, Contacted=amber, Qualified=emerald, Converted=violet, Lost=rose)

### Testing
14 tests across 3 files cover:
- KPI card rendering with correct values
- Derived value computation (open leads = sum of 3 statuses)
- Chart presence
- Quick actions rendering
- Loading skeleton state
- Null data graceful handling
