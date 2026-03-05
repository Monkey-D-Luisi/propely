# Epic P7 -- Intelligence & Analytics

## Overview

Finalize Propely for production launch by building actionable dashboards, AI conversation intelligence (context/memory for multi-turn interactions), proactive AI suggestions, and a final design refinement pass. This phase takes the feature-complete platform from Phases P0--P5 and adds the intelligence, observability, and polish required for real-world production use.

**Target:** A production-ready platform with data-driven dashboards, AI that remembers context across commands, proactive business suggestions, and a polished, consistent UI.

## Service Ownership

| Capability | Service |
|---|---|
| Dashboard analytics aggregation API | `services/orgs-api` |
| Cross-service data fetching (properties, contacts, appointments) | `services/orgs-api` via NuGet SDK clients |
| Conversation context & memory | `services/ai-api` |
| Proactive suggestion engine | `services/ai-api` |
| Frontend dashboard, suggestion feed | `apps/web` |

## Tasks

| # | Title | Status | Dependencies |
|---|---|---|---|
| 7.1 | Dashboard & Analytics | PENDING | 2.3, 4.3, 5.2 |
| 7.2 | Conversation Context & Memory | PENDING | 3.1 |
| 7.3 | Proactive AI Suggestions | PENDING | 3.1, 4.3 |
| 7.4 | Stitch Design Refinement Pass | PENDING | 7.1 |

---

## Task 7.1 -- Dashboard & Analytics

**Status:** PENDING
**Dependencies:** 2.3 (Properties API), 4.3 (Contacts & Leads API), 5.2 (Appointments API)

### Goal

Build agent and admin dashboards with KPIs, activity feeds, and cross-service analytics. Agents see their own performance; admins/owners see branch-wide metrics.

### Scope

**In scope:**
- Analytics aggregation endpoints in `orgs-api` that query other services via SDK clients
- Agent dashboard: my properties by status (pie chart), my leads by status (funnel), upcoming appointments (list), recent activity feed
- Admin dashboard: branch-wide metrics — total properties, active listings, leads this month, conversion rate, appointments this week, agent leaderboard
- Owner dashboard: cross-branch comparison — properties per branch, leads per branch, revenue pipeline
- KPI cards: total active listings, new leads this week, appointments today, average days-to-sale
- Date range filtering (this week, this month, this quarter, custom)
- Stitch design for all dashboard variants (agent, admin, owner)
- i18n (en, es)

**Out of scope:**
- Real-time WebSocket updates (polling with SWR revalidation is sufficient)
- Custom report builder
- Data export (CSV/PDF) — future enhancement

### Acceptance Criteria

- [ ] **AC1:** Agent dashboard shows: my properties by status, my leads by status, upcoming appointments, recent activity
- [ ] **AC2:** Admin dashboard shows: branch KPIs, agent leaderboard, branch-wide charts
- [ ] **AC3:** Owner dashboard shows: cross-branch comparison table
- [ ] **AC4:** KPI cards update based on date range filter
- [ ] **AC5:** Analytics API aggregates data from properties-api, contacts-api, appointments-api via SDK clients
- [ ] **AC6:** Dashboard data is cached (Redis, 5-minute TTL) to avoid hammering downstream services
- [ ] **AC7:** Stitch designs exist for agent, admin, and owner dashboard variants
- [ ] **AC8:** Responsive layout: cards reflow on mobile
- [ ] **AC9:** Component tests for all dashboard components
- [ ] **AC10:** i18n keys for en + es

### Implementation Steps

1. Define use cases for each dashboard role
2. Create analytics aggregation endpoints in `orgs-api` (Application layer)
3. Create SDK client calls to properties-api, contacts-api, appointments-api
4. Add Redis caching layer for aggregated metrics
5. Create Stitch designs for all dashboard variants
6. Implement frontend dashboard pages
7. Write component tests

### Files to Create/Modify

**Create:**
- `services/orgs-api/src/Propely.OrgsApi.Application/Analytics/Queries/GetAgentDashboard/`
- `services/orgs-api/src/Propely.OrgsApi.Application/Analytics/Queries/GetAdminDashboard/`
- `services/orgs-api/src/Propely.OrgsApi.Application/Analytics/Queries/GetOwnerDashboard/`
- `services/orgs-api/src/Propely.OrgsApi.Api/Controllers/AnalyticsController.cs`
- `apps/web/src/components/dashboard/AgentDashboard.tsx`
- `apps/web/src/components/dashboard/AdminDashboard.tsx`
- `apps/web/src/components/dashboard/OwnerDashboard.tsx`
- `apps/web/src/components/dashboard/KpiCard.tsx`
- `apps/web/src/components/dashboard/charts/` (property status pie, lead funnel, etc.)
- `apps/web/src/hooks/use-dashboard.ts`

**Modify:**
- `apps/web/src/app/[locale]/(dashboard)/page.tsx` (route to correct dashboard by role)
- `apps/web/src/messages/en.json`, `apps/web/src/messages/es.json`

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | Analytics query handlers aggregate data correctly | xUnit, mock SDK clients |
| Unit | Caching layer stores and retrieves results | xUnit, mock `ICacheService` |
| Unit | Dashboard components render KPI cards with data | Vitest + RTL |
| Unit | Date range filter changes trigger data refresh | Vitest + RTL |
| Integration | Analytics endpoint returns aggregated data | `WebApplicationFactory` |
| Manual | Visual comparison against Stitch designs | Dev environment |

### TDD Reminder

Write analytics query tests first with mocked SDK responses. Write frontend component tests for each card type. Implement to pass.

---

## Task 7.2 -- Conversation Context & Memory

**Status:** PENDING
**Dependencies:** 3.1 (AI Action Engine core)

### Goal

Add session-scoped conversation context to the AI action engine so users can reference previous commands and entities in follow-up requests.

### Scope

**In scope:**
- Conversation session: identified by `sessionId` (client-generated UUID, sent with each request)
- Context window: last N exchanges (default 10) stored server-side
- Entity memory: track last-mentioned property, contact, appointment per session
- Pronoun resolution: "it" → last mentioned entity, "her/him" → last mentioned contact, "there" → last mentioned address
- Demonstrative resolution: "that apartment" → last property of type Apartment, "the same client" → last contact
- `POST /v1/actions/execute` now accepts optional `sessionId` header or parameter
- Context stored in Redis with TTL (30 minutes of inactivity)
- Context is injected into OpenAI system prompt for function calling (recent exchanges as conversation history)
- Clear context: "forget everything" or "start over" resets the session

**Out of scope:**
- Cross-session memory (long-term user preferences)
- Conversation history UI beyond the command bar's recent commands (P3.10 already shows last 10)
- Multi-user conversation (each session is single-user)

### Acceptance Criteria

- [ ] **AC1:** `POST /v1/actions/execute` accepts optional `X-Session-Id` header
- [ ] **AC2:** If no session ID provided, each request is treated independently (stateless, backward-compatible)
- [ ] **AC3:** With session ID: previous exchanges are loaded from Redis and injected into OpenAI prompt
- [ ] **AC4:** "Reserve it for her" resolves "it" = last property, "her" = last contact
- [ ] **AC5:** "Change the price to 300k" (without specifying property) resolves to last-mentioned property
- [ ] **AC6:** "Show me more like it" resolves "it" to last-mentioned property and queries similar
- [ ] **AC7:** Context window stores last 10 exchanges (configurable)
- [ ] **AC8:** Context TTL is 30 minutes; expired sessions return to stateless mode
- [ ] **AC9:** "Start over" or "forget everything" clears the session context
- [ ] **AC10:** Context storage does not leak between tenants (key includes tenantId + userId)
- [ ] **AC11:** Unit tests for pronoun resolution with mocked context
- [ ] **AC12:** Integration tests for multi-turn conversations

### Implementation Steps

1. Create `IConversationContext` interface in `Application/Actions/Interfaces/`
2. Create `ConversationExchange` record — `{ UserInput, ActionType, Parameters, Result, Timestamp }`
3. Create `EntityMemory` record — `{ LastPropertyId, LastContactId, LastAppointmentId, LastAddress }`
4. Create `RedisConversationContext` in `Infrastructure/Actions/` — stores/loads exchanges and entity memory
5. Create `ContextualPromptBuilder` — injects conversation history into OpenAI system prompt
6. Create `PronounResolver` — resolves "it", "her", "him", "that" from entity memory
7. Update `OpenAiActionClassifier` to accept context and include it in the prompt
8. Update `ActionsController` to extract `X-Session-Id` and pass context through pipeline
9. Update frontend `useExecuteAction` to send session ID
10. Write tests

### Files to Create/Modify

**Create:**
- `services/ai-api/src/Propely.AiApi.Application/Actions/Interfaces/IConversationContext.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Models/ConversationExchange.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Models/EntityMemory.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/Actions/RedisConversationContext.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/Actions/ContextualPromptBuilder.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Services/PronounResolver.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Services/PronounResolverTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Infrastructure/Actions/RedisConversationContextTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Infrastructure/Actions/ContextualPromptBuilderTests.cs`
- `services/ai-api/tests/Propely.AiApi.IntegrationTests/Api/ConversationContextTests.cs`

**Modify:**
- `services/ai-api/src/Propely.AiApi.Infrastructure/Actions/OpenAiActionClassifier.cs` (inject context)
- `services/ai-api/src/Propely.AiApi.Api/Controllers/ActionsController.cs` (extract session ID)
- `services/ai-api/src/Propely.AiApi.Infrastructure/DependencyInjection.cs` (register context service)
- `apps/web/src/hooks/use-execute-action.ts` (add session ID to requests)
- `apps/web/src/hooks/use-command-bar.ts` (manage session ID lifecycle)

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `PronounResolver` resolves "it" to last property | xUnit |
| Unit | `PronounResolver` resolves "her" to last contact | xUnit |
| Unit | `PronounResolver` returns null when no context matches | xUnit |
| Unit | `RedisConversationContext` stores and retrieves exchanges | xUnit, mock Redis |
| Unit | `RedisConversationContext` respects window size limit (10) | xUnit |
| Unit | `RedisConversationContext` keys include tenantId + userId (isolation) | xUnit |
| Unit | `ContextualPromptBuilder` injects history into system prompt | xUnit |
| Unit | `ContextualPromptBuilder` handles empty history gracefully | xUnit |
| Integration | Multi-turn: "create apartment in Malaga" → "reserve it for Maria" resolves correctly | `WebApplicationFactory` |
| Integration | "Start over" clears context | `WebApplicationFactory` |
| Integration | Expired session returns to stateless mode | `WebApplicationFactory` |

### Security & Privacy

- Conversation context stored in Redis with TTL — automatically cleaned up
- Context is tenant-scoped: Redis key = `conversation:{tenantId}:{userId}:{sessionId}`
- Context may contain entity references (property IDs, contact names) — same privacy treatment as action execution
- No PII persisted beyond the TTL window

### TDD Reminder

Write `PronounResolver` tests first (all pronoun types, edge cases). Write context storage tests. Write multi-turn integration tests. Implement to pass.

---

## Task 7.3 -- Proactive AI Suggestions

**Status:** PENDING
**Dependencies:** 3.1 (AI Action Engine core), 4.3 (Contacts & Leads API)

### Goal

Generate AI-powered business suggestions based on data patterns — surface actionable insights to agents without them asking.

### Scope

**In scope:**
- Suggestion engine as a background service in `ai-api`
- Rule-based triggers (configurable):
  - "You have N leads that haven't been contacted in X days"
  - "Property AP-XXX has been in Draft for X days — ready to activate?"
  - "Contact Y has asked about N similar properties — consider a grouped viewing"
  - "You have no appointments scheduled this week"
  - "Lead conversion rate is below X% this month"
- Each trigger produces a `Suggestion` record: type, message (NL), action link (what to do), priority
- AI generates human-readable suggestion text from rule trigger data
- Suggestions API: `GET /v1/suggestions` returns current suggestions for the user
- Suggestions displayed in the command bar (subtle indicator) and as a notification feed
- Suggestion lifecycle: Created → Viewed → Dismissed / Actioned
- Runs periodically (every 15 minutes per tenant, configurable)

**Out of scope:**
- Push notifications (email, SMS, browser push)
- Custom rule creation by users
- ML-based predictions (rule-based only for now)

### Acceptance Criteria

- [ ] **AC1:** Background service runs periodically per tenant and generates suggestions
- [ ] **AC2:** "Stale leads" rule: triggers when leads > 7 days without contact (configurable)
- [ ] **AC3:** "Draft property" rule: triggers when property in Draft > 14 days
- [ ] **AC4:** "Grouped viewing" rule: triggers when a contact has interests in 3+ similar properties
- [ ] **AC5:** "Empty calendar" rule: triggers when agent has 0 appointments in next 7 days
- [ ] **AC6:** "Low conversion" rule: triggers when lead conversion rate < 20% for the month
- [ ] **AC7:** `GET /v1/suggestions` returns suggestions ordered by priority (high first)
- [ ] **AC8:** `POST /v1/suggestions/{id}/dismiss` marks suggestion as dismissed
- [ ] **AC9:** `POST /v1/suggestions/{id}/action` marks as actioned (logs what the user did)
- [ ] **AC10:** Dismissed suggestions don't reappear for the same trigger data
- [ ] **AC11:** Suggestions include a recommended action (e.g., "Contact Maria Garcia" links to contact detail)
- [ ] **AC12:** AI generates natural-sounding suggestion text from rule data
- [ ] **AC13:** Unit tests for each rule trigger
- [ ] **AC14:** Integration test for suggestion generation pipeline

### Implementation Steps

1. Create `Suggestion` entity in `Domain/Suggestions/`
2. Create `SuggestionRule` interface and implementations per rule type
3. Create suggestion generation service (background)
4. Create `SuggestionsController` with list/dismiss/action endpoints
5. Create frontend suggestion indicator in command bar
6. Create suggestion notification feed component
7. Write tests

### Files to Create/Modify

**Create:**
- `services/ai-api/src/Propely.AiApi.Domain/Suggestions/Suggestion.cs`
- `services/ai-api/src/Propely.AiApi.Domain/Suggestions/SuggestionType.cs`
- `services/ai-api/src/Propely.AiApi.Domain/Suggestions/SuggestionStatus.cs`
- `services/ai-api/src/Propely.AiApi.Application/Suggestions/Interfaces/ISuggestionRule.cs`
- `services/ai-api/src/Propely.AiApi.Application/Suggestions/Interfaces/ISuggestionRepository.cs`
- `services/ai-api/src/Propely.AiApi.Application/Suggestions/Rules/StaleLeadsRule.cs`
- `services/ai-api/src/Propely.AiApi.Application/Suggestions/Rules/DraftPropertyRule.cs`
- `services/ai-api/src/Propely.AiApi.Application/Suggestions/Rules/GroupedViewingRule.cs`
- `services/ai-api/src/Propely.AiApi.Application/Suggestions/Rules/EmptyCalendarRule.cs`
- `services/ai-api/src/Propely.AiApi.Application/Suggestions/Rules/LowConversionRule.cs`
- `services/ai-api/src/Propely.AiApi.Application/Suggestions/Services/SuggestionEngine.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/Suggestions/SuggestionGeneratorService.cs` (BackgroundService)
- `services/ai-api/src/Propely.AiApi.Infrastructure/Persistence/Configurations/SuggestionConfiguration.cs`
- `services/ai-api/src/Propely.AiApi.Infrastructure/Persistence/Repositories/SuggestionRepository.cs`
- `services/ai-api/src/Propely.AiApi.Api/Controllers/SuggestionsController.cs`
- `apps/web/src/components/command-bar/SuggestionIndicator.tsx`
- `apps/web/src/components/suggestions/SuggestionFeed.tsx`
- `apps/web/src/hooks/use-suggestions.ts`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Suggestions/Rules/StaleLeadsRuleTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Suggestions/Rules/DraftPropertyRuleTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Suggestions/Rules/GroupedViewingRuleTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Suggestions/Services/SuggestionEngineTests.cs`
- `services/ai-api/tests/Propely.AiApi.IntegrationTests/Api/SuggestionsControllerTests.cs`

**Modify:**
- `services/ai-api/src/Propely.AiApi.Infrastructure/Persistence/AppDbContext.cs` (add `DbSet<Suggestion>`)
- `services/ai-api/src/Propely.AiApi.Infrastructure/DependencyInjection.cs` (register suggestion services)
- `apps/web/src/components/command-bar/CommandBar.tsx` (add suggestion indicator)
- `apps/web/src/messages/en.json`, `apps/web/src/messages/es.json`

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Unit | `StaleLeadsRule` triggers for leads > 7 days without contact | xUnit, mock SDK |
| Unit | `StaleLeadsRule` does not trigger for recently contacted leads | xUnit |
| Unit | `DraftPropertyRule` triggers for properties in Draft > 14 days | xUnit |
| Unit | `GroupedViewingRule` triggers when contact has 3+ property interests | xUnit |
| Unit | `EmptyCalendarRule` triggers for 0 appointments in next 7 days | xUnit |
| Unit | `LowConversionRule` triggers for < 20% conversion | xUnit |
| Unit | `SuggestionEngine` runs all rules and deduplicates | xUnit |
| Integration | `GET /v1/suggestions` returns generated suggestions | `WebApplicationFactory` |
| Integration | `POST /v1/suggestions/{id}/dismiss` marks dismissed | `WebApplicationFactory` |

### Security & Privacy

- Suggestions reference entities by ID (property, contact) — no PII in suggestion text
- Suggestion text is generated by AI from anonymized trigger data
- Suggestions are scoped to the authenticated user + tenant

### TDD Reminder

Write rule trigger tests first (each rule with matching and non-matching data). Write engine deduplication tests. Implement to pass.

---

## Task 7.4 -- Stitch Design Refinement Pass

**Status:** PENDING
**Dependencies:** 7.1 (all major screens must exist)

### Goal

Full design audit and consistency pass across all screens using Stitch MCP. Regenerate designs where needed, fix pixel-level inconsistencies, and verify responsive behavior.

### Scope

**In scope:**
- Audit all existing screens in Stitch project (16786124142182555397)
- Identify inconsistencies: spacing, colors, typography, border radius, shadows
- Regenerate updated designs for screens that need refinement
- Download updated Stitch HTML to `.stitch-html/`
- Fix implementation to match updated designs
- Responsive spot-check: all screens on mobile (375px) and tablet (768px)
- Accessibility pass: color contrast (WCAG AA), focus indicators, ARIA attributes

**Out of scope:**
- New feature development
- Performance optimization
- Backend changes

### Acceptance Criteria

- [ ] **AC1:** All Stitch designs in project reviewed and updated where inconsistent
- [ ] **AC2:** All `.stitch-html/` files updated to latest designs
- [ ] **AC3:** Implementation matches Stitch designs pixel-for-pixel (verified by visual comparison)
- [ ] **AC4:** All screens responsive at 375px, 768px, and 1280px
- [ ] **AC5:** WCAG AA color contrast met on all text elements
- [ ] **AC6:** All interactive elements have visible focus indicators
- [ ] **AC7:** All forms have proper label associations and ARIA attributes
- [ ] **AC8:** No regressions in existing component tests

### Implementation Steps

1. List all screens in Stitch project
2. Compare each Stitch design against implementation
3. Document discrepancies
4. Regenerate Stitch designs where needed
5. Fix implementation to match
6. Run responsive checks
7. Run accessibility checks (aXe or similar)
8. Verify no test regressions

### Testing Plan

| Layer | What to test | Approach |
|---|---|---|
| Visual | All screens match Stitch designs | Manual comparison |
| Visual | Responsive layouts at 375px, 768px, 1280px | Browser dev tools |
| Accessibility | Color contrast | aXe browser extension |
| Accessibility | Focus indicators | Manual keyboard navigation |
| Regression | Existing component tests still pass | `npm test` |

### TDD Reminder

Run existing test suite before and after changes. Document visual changes in walkthrough.
