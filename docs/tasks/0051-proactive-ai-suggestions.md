# Task: 0051-proactive-ai-suggestions

## Metadata
- ID: 0051
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-08
- Related docs:
  - Walkthrough: `docs/walkthroughs/0051-proactive-ai-suggestions.md`
  - Epic: `docs/backlog/epic-P7-polish.md` (Task 7.3)

## Goal
Build a rule-based suggestion engine that evaluates live data from downstream services and surfaces actionable business insights to agents on the dashboard.

## Context
Agents had no proactive guidance from the system. The dashboard showed raw metrics but didn't interpret them or suggest actions. A rule-based engine was needed to bridge the gap between data and actionable next steps.

## Scope
### In scope
- `ISuggestionRule` interface for pluggable rules
- 5 rules: StaleLeads, DraftProperty, EmptyCalendar, LowConversion, GroupedViewing
- `SuggestionEngine` that orchestrates rules with fault tolerance (failing rules don't block others)
- `GET /v1/suggestions` API endpoint
- Frontend `SuggestionFeed` component on dashboard
- i18n (en, es)
- 20 backend + 4 frontend unit tests

### Out of scope
- Database-persisted suggestions (computed on-demand from live data)
- Background service (future enhancement)
- Dismiss/action tracking
- Push notifications
- AI-generated suggestion text (rule templates with interpolated data)

## Requirements
- R1: Each rule evaluates data from SDK clients and returns 0+ suggestions
- R2: Suggestions have type, priority (High/Medium/Low), message, optional action URL+label
- R3: Engine runs all rules and returns results ordered by priority (high first)
- R4: If a rule fails (service unavailable), engine continues with remaining rules
- R5: StaleLeads triggers at 5+ new leads
- R6: DraftProperty triggers at 3+ draft properties
- R7: EmptyCalendar triggers when 0 upcoming appointments in 7 days
- R8: LowConversion triggers below 20% rate with minimum 5 lead sample
- R9: GroupedViewing triggers when contact has 3+ property interests

## Changes

### New backend files (ai-api)
| File | Purpose |
|------|---------|
| `Domain/Suggestions/Suggestion.cs` | Record: Type, Priority, Message, ActionUrl, ActionLabel |
| `Domain/Suggestions/SuggestionType.cs` | Enum: StaleLeads, DraftProperty, EmptyCalendar, LowConversion, GroupedViewing |
| `Domain/Suggestions/SuggestionPriority.cs` | Enum: Low, Medium, High |
| `Application/Suggestions/Interfaces/ISuggestionRule.cs` | Rule interface: Type, EvaluateAsync |
| `Application/Suggestions/Rules/StaleLeadsRule.cs` | Checks for 5+ new leads via LeadsApiClient |
| `Application/Suggestions/Rules/DraftPropertyRule.cs` | Checks for 3+ draft properties |
| `Application/Suggestions/Rules/EmptyCalendarRule.cs` | Checks for 0 upcoming appointments |
| `Application/Suggestions/Rules/LowConversionRule.cs` | Checks conversion rate below 20% |
| `Application/Suggestions/Rules/GroupedViewingRule.cs` | Checks contacts with 3+ property interests |
| `Application/Suggestions/Rules/SuggestionEngine.cs` | Orchestrates rules, orders by priority |
| `Application/Suggestions/Queries/GetSuggestions/GetSuggestionsQuery.cs` | MediatR query + handler |
| `Api/Controllers/SuggestionsController.cs` | GET /v1/suggestions with RequireViewer policy |

### New test files (ai-api)
| File | Tests |
|------|-------|
| `StaleLeadsRuleTests.cs` | 4 tests: above/below threshold, no new leads, type check |
| `DraftPropertyRuleTests.cs` | 3 tests: above/below threshold, no draft |
| `EmptyCalendarRuleTests.cs` | 2 tests: no upcoming, has upcoming |
| `LowConversionRuleTests.cs` | 4 tests: below/above threshold, too few leads, exactly at threshold |
| `GroupedViewingRuleTests.cs` | 3 tests: enough interests, few interests, no contacts |
| `SuggestionEngineTests.cs` | 4 tests: aggregation, priority ordering, fault tolerance, empty result |

### Modified backend files
| File | Change |
|------|--------|
| `Infrastructure/DependencyInjection.cs` | Registered 5 rules + SuggestionEngine |

### New frontend files
| File | Purpose |
|------|---------|
| `hooks/use-suggestions.ts` | Fetches from /v1/suggestions via aiApiFetch |
| `components/suggestions/SuggestionFeed.tsx` | Renders suggestion cards with priority styling |
| `components/suggestions/__tests__/SuggestionFeed.test.tsx` | 4 tests: renders, loading, empty, action links |

### Modified frontend files
| File | Change |
|------|--------|
| `app/[locale]/(dashboard)/page.tsx` | Added SuggestionFeed to dashboard |
| `messages/en.json` | Added `suggestions` namespace |
| `messages/es.json` | Added `suggestions` namespace |

## Verification
```bash
dotnet test services/ai-api/tests/Propely.AiApi.UnitTests  # 590 tests pass
cd apps/web && npm test                                      # 842 tests pass
```
