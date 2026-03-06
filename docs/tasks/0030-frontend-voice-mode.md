# Task: 0030-frontend-voice-mode

## Metadata
- ID: 0030
- Type: Standard
- Status: DONE
- Owner: Agent
- Created: 2026-03-05
- Related docs:
  - Walkthrough: `docs/walkthroughs/0030-frontend-voice-mode.md`
  - Related: `docs/tasks/0027-voice-input-stt.md` (backend STT endpoint)

## Goal
Add voice input capability to the command bar so users can interact with the AI action engine via spoken commands. A microphone button records audio, sends it for transcription via the AI API voice endpoint, and feeds the resulting text into the command pipeline.

## Context
The Propely command bar (Ctrl+K) already supports text-based natural language commands. This task extends it with voice input, enabling hands-free interaction. The backend `POST /v1/voice/execute` endpoint (implemented in Task 3.8) accepts multipart audio and returns a transcription plus optional action result.

## Scope
### In scope
- `useVoiceInput` hook -- MediaRecorder lifecycle, permission management, audio chunk collection
- `useAudioWaveform` hook -- Web Audio API AnalyserNode for live frequency data
- `useExecuteVoiceAction` hook -- multipart POST to `/v1/voice/execute` with auth token refresh
- `VoiceMicButton` component -- mic button with idle/recording/processing states
- `AudioWaveform` component -- canvas-based waveform visualization
- Integration into `CommandBar.tsx` and `CommandInput.tsx`
- i18n translations (English and Spanish)
- Unit tests for all new components and hooks

### Out of scope
- Backend voice endpoint implementation (already done in Task 3.8)
- Speech-to-text model selection
- Offline voice processing

## Technical Design

### Hooks
1. **`use-voice-input.ts`** -- manages `MediaRecorder` lifecycle
   - States: `idle | requesting-permission | recording | processing`
   - Rejects recordings < 0.5 seconds
   - Releases media stream tracks on stop

2. **`use-audio-waveform.ts`** -- connects `MediaStream` to `AnalyserNode`
   - Uses `requestAnimationFrame` for 60fps updates
   - Returns `Uint8Array` frequency data for visualization

3. **`use-execute-voice-action.ts`** -- multipart form-data upload
   - Uses `FormData` with audio blob (not `aiApiFetch` which forces JSON content-type)
   - Handles 401 retry with token refresh

### Components
4. **`VoiceMicButton.tsx`** -- microphone icon button
   - Idle: slate mic icon
   - Recording: pulsing red ring + waveform
   - Processing: spinner
   - Permission denied: alert popover
   - 48x48 minimum tap target for mobile

5. **`AudioWaveform.tsx`** -- `<canvas>` frequency bars
   - 80x24 default size
   - Up to 16 bars from frequency data

### Integration
- `CommandInput` extended with `trailingSlot` and `externalValue` props
- `CommandBar` wires `VoiceMicButton` into the input area
- On voice complete: transcription fills input, auto-submits as command

## Files Changed
- `apps/web/src/hooks/use-voice-input.ts` (new)
- `apps/web/src/hooks/use-audio-waveform.ts` (new)
- `apps/web/src/hooks/use-execute-voice-action.ts` (new)
- `apps/web/src/components/command-bar/VoiceMicButton.tsx` (new)
- `apps/web/src/components/command-bar/AudioWaveform.tsx` (new)
- `apps/web/src/components/command-bar/CommandBar.tsx` (modified)
- `apps/web/src/components/command-bar/CommandInput.tsx` (modified)
- `apps/web/messages/en.json` (modified -- added `commandBar.voice`)
- `apps/web/messages/es.json` (modified -- added `commandBar.voice`)
- `apps/web/src/components/command-bar/__tests__/VoiceMicButton.test.tsx` (new)
- `apps/web/src/components/command-bar/__tests__/AudioWaveform.test.tsx` (new)
- `apps/web/src/components/command-bar/__tests__/use-voice-input.test.ts` (new)

## Quality Gate
```bash
cd apps/web && npm run build && npm test
```
All tests must pass. No TypeScript errors.
