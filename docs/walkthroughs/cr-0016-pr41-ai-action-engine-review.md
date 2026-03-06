# Walkthrough: cr-0016 — PR #41 AI Action Engine Review

## References
- Task: `docs/tasks/cr-0016-pr41-ai-action-engine-review.md`
- PR: #41 `feat(P3): AI Action Engine`

## Changes Made

### A1: VoiceMicButton render-phase side effect → useEffect
- **File**: `apps/web/src/components/command-bar/VoiceMicButton.tsx`
- Replaced `queueMicrotask` call in render body with a `useEffect` that fires `onAudioReady` once when audioBlob becomes available.

### A2: Deduplicate token refresh logic
- **File**: `apps/web/src/lib/api.ts` — exported `tryRefreshToken`
- **File**: `apps/web/src/hooks/use-execute-voice-action.ts` — removed local `tryRefreshToken`, imported from `api.ts`

### A3 + A4: Align frontend types to backend DTOs
- **File**: `apps/web/src/hooks/use-execute-action.ts` — added `success`, `errors`, `confidence` to `ActionResult`
- **File**: `apps/web/src/hooks/use-execute-voice-action.ts` — aligned `VoiceExecuteResult` fields to match `VoiceExecuteResultDto` (`transcribedText`, `language`, `durationMs`, `action`)

### A5: CommandResult success/failure branching
- **File**: `apps/web/src/components/command-bar/CommandResult.tsx` — added `result.success === false` error branch

### A6: Remove exception message leaking
- **Files**: `ExtractFromTextActionHandler.cs`, `GenerateCopyActionHandler.cs`, `ExtractFromPhotosActionHandler.cs`
- Replaced `ex.Message` in error arrays with generic messages

### B1: Restore Swagger JWT bearer security
- **File**: `services/ai-api/src/Propely.AiApi.Api/DependencyInjection.cs` — restored `AddSecurityDefinition` and `AddSecurityRequirement` for Bearer scheme

### B2: Focus rings on CommandHistory buttons
- **File**: `apps/web/src/components/command-bar/CommandHistory.tsx` — added `focus:ring-2 focus:ring-primary-600 focus:ring-offset-2`

### B3: Task status updates
- **Files**: `docs/tasks/0027-0030` — updated Status from `IN_PROGRESS` to `DONE`

### B4: Fix VoiceMicButton error type assertion
- **File**: `apps/web/src/components/command-bar/VoiceMicButton.tsx` — removed `transcriptionFailed` from type assertion (only `useVoiceInput` errors can appear here)

## Validation
- `dotnet build services/ai-api/Propely.AiApi.sln`
- `dotnet test services/ai-api/Propely.AiApi.sln`
- `cd apps/web && npm run build && npm test`
- CI checks monitored via `gh pr checks 41 --watch`
