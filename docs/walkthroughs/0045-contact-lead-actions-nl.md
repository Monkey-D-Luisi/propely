# Walkthrough: 0045-contact-lead-actions-nl

## Task Reference
- Task: `docs/tasks/0045-contact-lead-actions-nl.md`
- Walkthrough: `docs/walkthroughs/0045-contact-lead-actions-nl.md`
- Branch/PR: `epic/P3-ai-action-engine`
- Date: `2026-03-07`

## Summary
Implemented 5 NL action handlers for contact and lead management in ai-api. Handlers call ContactsApi SDK client (IContactsApiClient, ILeadsApiClient) to create leads, create contacts, qualify leads, convert leads, and query leads via natural language commands.

## Context
- Background: AI Action Engine had property/operation/content/voice actions but no contact/lead actions
- Problem statement: Users could not manage contacts/leads through natural language commands
- Constraints: ContactsApi SDK client (P4.5) needed to be DONE first; follow existing handler patterns

## Decisions & Trade-offs
1. **Contact role mapping**: Map Spanish roles (comprador, vendedor, inquilino, propietario) to English enum values (Buyer, Seller, Tenant, Landlord) using vocabulary lookup
2. **Lead qualification flow**: QualifyLead handler changes status to "Qualified" via SDK, following the same pattern as other status-change handlers
3. **Lead conversion**: ConvertLead handler delegates to SDK's ConvertLeadAsync, which handles the full conversion flow server-side
4. **Query results pagination**: QueryLeads returns max 20 results per page with summary text, consistent with QueryProperties pattern

## Implementation Notes
- All 5 handlers follow the same pattern: extract params with ParameterExtractor, validate required fields, call SDK, return ActionResult.Ok/Fail
- Added 5 tool definitions to ToolDefinitions.cs with Spanish parameter aliases
- ActionRouter switch cases added for all 5 action types
- ContactsApi SDK registered in DI (IContactsApiClient + ILeadsApiClient)

## Commands Run
```bash
dotnet build services/ai-api/Propely.AiApi.sln   # 0 errors, 0 warnings
dotnet test services/ai-api/Propely.AiApi.sln     # All tests passed
```

## Files Created
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/ContactActions/CreateLeadActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/ContactActions/CreateContactActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/ContactActions/QualifyLeadActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/ContactActions/ConvertLeadActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Commands/ContactActions/QueryLeadsActionCommand.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Handlers/ContactActions/CreateLeadActionHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Handlers/ContactActions/CreateContactActionHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Handlers/ContactActions/QualifyLeadActionHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Handlers/ContactActions/ConvertLeadActionHandler.cs`
- `services/ai-api/src/Propely.AiApi.Application/Actions/Handlers/ContactActions/QueryLeadsActionHandler.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Handlers/ContactActions/CreateLeadActionHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Handlers/ContactActions/CreateContactActionHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Handlers/ContactActions/QualifyLeadActionHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Handlers/ContactActions/ConvertLeadActionHandlerTests.cs`
- `services/ai-api/tests/Propely.AiApi.UnitTests/Application/Actions/Handlers/ContactActions/QueryLeadsActionHandlerTests.cs`

## Files Modified
- `services/ai-api/src/Propely.AiApi.Application/Propely.AiApi.Application.csproj` (added ContactsApi.Client reference)
- `services/ai-api/src/Propely.AiApi.Infrastructure/AI/ToolDefinitions.cs` (5 new tool definitions)
- `services/ai-api/src/Propely.AiApi.Infrastructure/AI/ActionRouter.cs` (5 new switch cases)
- `services/ai-api/src/Propely.AiApi.Api/Program.cs` (ContactsApi SDK DI registration)

## Tests
### Unit
- 5 handler test files with 3-5 tests each covering: happy path, missing required params, SDK failure
- ActionRouter tests updated (5 new action type dispatch tests)
- ToolDefinitions test updated for 20 total tools

## Checklist
- [x] Task scope matches `docs/tasks/0045-contact-lead-actions-nl.md`
- [x] Tests updated and passing
- [x] Docs updated where relevant
- [x] No secrets committed (.env only / templates for examples)
