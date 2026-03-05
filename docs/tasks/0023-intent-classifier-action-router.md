# Task: 0023-intent-classifier-action-router

## Metadata
- ID: 0023
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-05
- Related docs:
  - Walkthrough: `docs/walkthroughs/0023-intent-classifier-action-router.md`
  - Epic: `docs/backlog/epic-P3-ai-engine.md` (Task 3.1)

## Goal
Implement the core AI Action Engine infrastructure for the ai-api service: intent classification via OpenAI function calling, action routing via MediatR, and the `POST /v1/actions/execute` API endpoint. This is the foundational plumbing that all subsequent AI action tasks (3.2, 3.3, etc.) depend on.

## Context
The ai-api service previously had only basic text-in/text-out OpenAI integration (IOpenAiService.GenerateTextAsync) and a ParseWorkItem command. This task introduces the full AI Action Engine pattern: user text is classified into a structured intent using OpenAI function calling, then routed to the appropriate action handler. The architecture supports any number of action types that can be plugged in incrementally.

## Scope
### In scope
- Domain: ActionType enum, ActionResult/ActionResult<T> records, ClassifiedIntent record
- Application: IIntentClassifier and IActionRouter interfaces, ExecuteActionCommand/Handler/Validator
- Infrastructure: OpenAiIntentClassifier (OpenAI function calling), ActionRouter (placeholder routing), ToolDefinitions (static tool schemas)
- API: ActionsController with POST /v1/actions/execute, ActionDtos, ExecuteActionRequestValidator
- DI registration of IIntentClassifier and IActionRouter
- Unit tests for all components (handler, router, tool definitions, classifier, validator, domain records)
- Rate limiting for the actions/execute endpoint

### Out of scope
- Individual action handler implementations (Task 3.2+)
- Voice input processing (Task 3.x)
- Integration tests with live OpenAI API
- Lead, contact, and appointment action tool definitions (future tasks)

## Requirements
- R1: IIntentClassifier classifies natural language text into ActionType + parameters using OpenAI function calling
- R2: Tool definitions cover 9 property-related actions (create, update, query, status change, copy, extract, reserve, close, archive)
- R3: IActionRouter maps classified intents to action handlers (placeholder for now)
- R4: ExecuteActionCommandHandler orchestrates classify -> validate -> route pipeline
- R5: Low confidence (< 0.5) returns a "please be more specific" message
- R6: Unknown intent returns a "didn't understand" message with examples
- R7: Classifier and router exceptions are caught and return graceful failures
- R8: POST /v1/actions/execute endpoint requires authentication (JWT)
- R9: Request validation: text required, max 2000 characters
- R10: Rate limited to 20 requests per minute per IP

## Acceptance Criteria
- [x] All 51 new unit tests pass
- [x] All 139 total unit tests pass (no regression)
- [x] Architecture tests pass (domain has no framework deps, clean dependency flow)
- [x] Solution builds with 0 errors
- [x] ActionsController exposes POST /v1/actions/execute
- [x] OpenAiIntentClassifier uses ChatTool.CreateFunctionTool for intent classification
- [x] ActionRouter returns placeholder results for all known action types
- [x] ExecuteActionCommandHandler handles Unknown, low confidence, and exception cases
