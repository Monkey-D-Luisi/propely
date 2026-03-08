# Walkthrough: 0051-proactive-ai-suggestions

## Overview
Added a rule-based proactive suggestion engine to the ai-api service. The engine evaluates live data from properties, contacts, and appointments services via SDK clients and generates actionable suggestions for agents. Suggestions appear on the dashboard.

## Architecture
On-demand computation (no database persistence):
```
GET /v1/suggestions → SuggestionsController
  → GetSuggestionsQuery (MediatR)
    → SuggestionEngine.GenerateAsync(tenantId)
      → StaleLeadsRule.EvaluateAsync (→ ILeadsApiClient.CountByStatusAsync)
      → DraftPropertyRule.EvaluateAsync (→ IPropertiesApiClient.CountByStatusAsync)
      → EmptyCalendarRule.EvaluateAsync (→ IAppointmentsApiClient.CountUpcomingAsync)
      → LowConversionRule.EvaluateAsync (→ ILeadsApiClient.CountByStatusAsync)
      → GroupedViewingRule.EvaluateAsync (→ IContactsApiClient.GetContactsAsync)
    → Sort by priority (High first), return
```

## Rules

| Rule | Trigger | Priority | Data Source |
|------|---------|----------|-------------|
| StaleLeads | 5+ leads in "New" status | High | LeadsApiClient.CountByStatusAsync |
| DraftProperty | 3+ properties in "Draft" status | Medium | PropertiesApiClient.CountByStatusAsync |
| EmptyCalendar | 0 upcoming appointments (7 days) | Medium | AppointmentsApiClient.CountUpcomingAsync |
| LowConversion | Conversion rate < 20% (min 5 leads) | High | LeadsApiClient.CountByStatusAsync |
| GroupedViewing | Contact with 3+ property interests | Medium | ContactsApiClient.GetContactsAsync |

## Fault Tolerance
The SuggestionEngine wraps each rule evaluation in a try-catch. If a downstream service is unavailable, the failing rule is logged as a warning and skipped — other rules continue executing. This prevents a single service outage from breaking the entire suggestions feature.

## Frontend
- `SuggestionFeed` renders suggestion cards with priority-based styling (red for High, amber for Medium, sky for Low)
- Each card shows an icon, message text, and an optional action link
- Loading state shows animated skeleton cards
- Empty state shows "All caught up!" message
