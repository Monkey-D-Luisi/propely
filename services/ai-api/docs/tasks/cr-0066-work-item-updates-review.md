# Code Review: cr-0066-work-item-updates-review

## Metadata
- PR: #68 - feat: implement soft delete and updates for work items
- PR Link: https://github.com/Monkey-D-Luisi/ai-api-template/pull/68
- Target Branch: main
- CI Status: Mixed (Claude failed, Build passed)
- Review Date: 2026-02-03

## Changed Files
- `docs/tasks/0015-work-item-updates-deletion.md`
- `docs/walkthroughs/0015-work-item-updates-deletion.md`
- `src/SaasTemplate.AiApi.Api/Configuration/AuthorizationPolicies.cs`
- `src/SaasTemplate.AiApi.Api/Controllers/WorkItemsController.cs`
- `src/SaasTemplate.AiApi.Application/WorkItems/Commands/UpdateWorkItemCommandHandler.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemEventProjector.cs`
- `src/SaasTemplate.AiApi.Infrastructure/Persistence/Repositories/WorkItemRepository.cs`
- `src/SaasTemplate.AiApi.Domain/WorkItems/WorkItem.cs`
- `tests/SaasTemplate.AiApi.IntegrationTests/Api/WorkItemsControllerTests.cs`
- `tests/SaasTemplate.AiApi.IntegrationTests/Messaging/WorkItemProjectorTests.cs`
- (And others)

## Review Sources
- Review Comments: 12 (Copilot) + 7 (Gemini)
- Reviews: 2
- Issue Comments: 2

## Comment Resolution Plan

### MUST_FIX
- [x] [Copilot: Authorization policies missing in DependencyInjection](https://github.com/Monkey-D-Luisi/ai-api-template/pull/68#discussion_r2757937298)
  - File: `src/SaasTemplate.AiApi.Api/Configuration/DependencyInjection.cs` (Implicitly)
  - Proposed change: Register `CanUpdateWorkItem` and `CanDeleteWorkItem`.
- [x] [Copilot: Missing Integration Tests for PUT/DELETE endpoints](https://github.com/Monkey-D-Luisi/ai-api-template/pull/68#discussion_r2757937377)
  - File: `tests/SaasTemplate.AiApi.IntegrationTests/Api/WorkItemsControllerTests.cs`
  - Proposed change: Add `PUT` and `DELETE` test cases.
- [x] [Copilot: Missing Projector Tests for Update/Delete](https://github.com/Monkey-D-Luisi/ai-api-template/pull/68#discussion_r2757937250)
  - File: `tests/SaasTemplate.AiApi.IntegrationTests/Messaging/WorkItemProjectorTests.cs`
  - Proposed change: Add tests for `WorkItemUpdatedV1` and `WorkItemDeletedV1`.
- [x] [Gemini/Copilot: Docs Discrepancy (Soft vs Hard Delete)](https://github.com/Monkey-D-Luisi/ai-api-template/pull/68#discussion_r2757897925)
  - File: `docs/tasks/0015-work-item-updates-deletion.md`
  - Proposed change: Update to reflect Soft Delete implementation.
- [x] [Copilot: Duplicate Walkthrough files](https://github.com/Monkey-D-Luisi/ai-api-template/pull/68#discussion_r2757937352)
  - File: `docs/walkthroughs/0015-work-item-updates-deletion.md`
  - Proposed change: DELETE this file. Keep `task-0015-work-item-updates.md`.

### SHOULD_FIX
- [x] [Gemini: Use nameof for WorkItemStatus.Deleted](https://github.com/Monkey-D-Luisi/ai-api-template/pull/68#discussion_r2757897954)
  - File: `src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemEventProjector.cs`
  - Proposed change: `readModel.Status = nameof(WorkItemStatus.Deleted);`
- [x] [Copilot: Guard clause for updating deleted items](https://github.com/Monkey-D-Luisi/ai-api-template/pull/68#discussion_r2757937401)
  - File: `src/SaasTemplate.AiApi.Domain/WorkItems/WorkItem.cs`
  - Proposed change: Add check `if (Status == WorkItemStatus.Deleted) throw ...` in `Update()`.
- [x] [Gemini/Copilot: Typo in CanDeleteWorkItem comment](https://github.com/Monkey-D-Luisi/ai-api-template/pull/68#discussion_r2757897934)
  - File: `src/SaasTemplate.AiApi.Api/Configuration/AuthorizationPolicies.cs`
  - Proposed change: Fix copy-paste error "for reading" -> "for deleting".

### SUGGESTION
- [x] [Gemini/Copilot: Remove thought process comments](https://github.com/Monkey-D-Luisi/ai-api-template/pull/68#discussion_r2757897944)
  - File: `src/SaasTemplate.AiApi.Application/WorkItems/Commands/UpdateWorkItemCommandHandler.cs`
  - Action: Remove comments.
- [x] [Gemini: Fix XML Comment placement](https://github.com/Monkey-D-Luisi/ai-api-template/pull/68#discussion_r2757897948)
  - File: `src/SaasTemplate.AiApi.Application/WorkItems/Interfaces/IWorkItemRepository.cs`
  - Action: Move comment.
- [x] [Copilot: Fix indentation](https://github.com/Monkey-D-Luisi/ai-api-template/pull/68#discussion_r2757937331)
  - File: `src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemEventProjector.cs`
  - Action: Fix indentation.
- [x] [Gemini/Copilot: Remove trivial comments](https://github.com/Monkey-D-Luisi/ai-api-template/pull/68#discussion_r2757937382)
  - File: `src/SaasTemplate.AiApi.Infrastructure/Messaging/WorkItemEventProjector.cs`
  - Action: Remove "Stored as string?" comment.

### QUESTION
- None

### OUT_OF_SCOPE
- None

## Implementation Notes
- Will delete `docs/walkthroughs/0015-work-item-updates-deletion.md` as requested.
