# Task: 0045-contact-lead-actions-nl

## Metadata
- ID: 0045
- Type: Standard
- Status: DOING
- Owner: Agent
- Created: 2026-03-07
- Related docs:
  - Walkthrough: `docs/walkthroughs/0045-contact-lead-actions-nl.md`
  - Epic: `docs/backlog/epic-P3-ai-action-engine.md` (Task 3.4)

## Goal
Implement action handlers for contact and lead management through natural language commands in the AI Action Engine.

## Context
The AI Action Engine (P3) already has property actions, operation actions, content generation, and voice input. Contact & Lead NL actions were blocked on the ContactsApi SDK Client (P4.5), which is now DONE. This task adds 5 new action handlers that call the ContactsApi SDK to create leads, create contacts, qualify leads, convert leads, and query leads via natural language.

## Scope
### In scope
- `CreateLeadActionHandler` -- creates lead via ILeadsApiClient
- `CreateContactActionHandler` -- creates contact via IContactsApiClient
- `QualifyLeadActionHandler` -- changes lead status to Qualified via SDK
- `ConvertLeadActionHandler` -- triggers lead conversion flow via SDK
- `QueryLeadsActionHandler` -- searches/filters leads via SDK
- ToolDefinitions for all 5 actions (OpenAI function calling schemas)
- ActionRouter switch cases for all 5 actions
- ContactsApi SDK client registration in DI

### Out of scope
- ContactEntityResolver (entity resolution by name/phone -- deferred to 3.9 prompt engineering)
- Frontend lead management
- Contacts domain logic changes

## Acceptance Criteria
- AC1: CreateLead handler creates lead via SDK with extracted parameters
- AC2: CreateContact handler creates contact via SDK with role and phone
- AC3: QualifyLead handler changes lead status to Qualified via SDK
- AC4: ConvertLead handler triggers conversion via SDK
- AC5: QueryLeads handler queries leads with status/property filters
- AC6: All 5 tool definitions added to ToolDefinitions.cs
- AC7: All 5 action types routed in ActionRouter.cs
- AC8: ContactsApi SDK registered in DI
- AC9: Unit tests for each handler with mocked SDK clients
- AC10: All handlers propagate tenant context

## Implementation Steps
1. Add ContactsApi.Client project references
2. Register ContactsApi SDK in DI
3. Write unit tests for all 5 handlers (TDD: Red)
4. Create 5 command records in Commands/ContactActions/
5. Create 5 handler classes in Handlers/ContactActions/
6. Add 5 tool definitions to ToolDefinitions.cs
7. Update ActionRouter.cs switch cases
8. Run tests (TDD: Green)
9. Build and verify

## Definition of Done Checklist
- [ ] Acceptance criteria met
- [ ] Build passes
- [ ] Tests added/updated and pass
- [ ] Formatting/analyzers pass
- [ ] No secrets committed
- [ ] Walkthrough updated
