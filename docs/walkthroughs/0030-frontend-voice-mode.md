# Walkthrough: 0030-frontend-voice-mode

## Summary
This walkthrough documents the implementation of voice input mode for the Propely command bar. Users can now click a microphone button to record voice commands, which are transcribed and executed as natural language actions.

## Architecture

### Data Flow
```
User clicks mic -> getUserMedia (permission) -> MediaRecorder captures audio
  -> stopRecording -> Blob created -> POST /v1/voice/execute (multipart)
  -> Transcription returned -> Text fills command input -> Auto-submit as action
```

### Component Hierarchy
```
CommandBar
  +-- CommandInput
  |     +-- VoiceMicButton (via trailingSlot prop)
  |           +-- AudioWaveform (during recording)
  +-- CommandResult / ConfirmationPanel / CommandHistory
```

## Hooks

### useVoiceInput (`apps/web/src/hooks/use-voice-input.ts`)
Manages the full `MediaRecorder` lifecycle with four states:
- **idle**: no recording in progress
- **requesting-permission**: waiting for `getUserMedia` response
- **recording**: actively recording audio chunks
- **processing**: recording stopped, blob ready for upload

Key behaviors:
- Requests microphone permission lazily (on first click, not on mount)
- Collects audio data via `ondataavailable` events
- Rejects recordings shorter than 500ms to avoid accidental taps
- Releases all media stream tracks on stop to free the microphone
- Tracks `permissionDenied` state separately for UI feedback

### useAudioWaveform (`apps/web/src/hooks/use-audio-waveform.ts`)
Connects a `MediaStream` to the Web Audio API for live visualization:
- Creates an `AudioContext` and `AnalyserNode` with `fftSize=64`
- Uses `requestAnimationFrame` to poll frequency data at 60fps
- Returns a new `Uint8Array` copy each frame so React detects changes
- Cleans up all Web Audio resources when the stream disconnects

### useExecuteVoiceAction (`apps/web/src/hooks/use-execute-voice-action.ts`)
Handles the multipart upload to the voice endpoint:
- Cannot use `aiApiFetch` because it forces `content-type: application/json`
- Uses raw `fetch` with `FormData` (browser sets multipart boundary automatically)
- Attaches Bearer token and X-Org-Id headers manually
- Implements 401 retry with token refresh (same pattern as `aiApiFetch`)

## Components

### VoiceMicButton (`apps/web/src/components/command-bar/VoiceMicButton.tsx`)
A compound component that composes `useVoiceInput` and `useAudioWaveform`:
- **Idle state**: slate-colored microphone icon, hover effect
- **Recording state**: red background, pulsing ring animation (`animate-pulse` + `ring-red-500`), waveform below
- **Processing state**: spinning loader, button disabled
- **Permission denied**: red alert popover with guidance text
- **Error state**: amber alert popover
- Minimum 48x48 touch target for mobile accessibility
- Optional haptic feedback via `navigator.vibrate(50)`
- Calls `onAudioReady(blob)` when recording completes

### AudioWaveform (`apps/web/src/components/command-bar/AudioWaveform.tsx`)
A canvas-based visualization that renders frequency data as vertical bars:
- Default size 80x24px (compact, fits next to mic button)
- Up to 16 bars with rounded caps
- DPI-aware rendering via `devicePixelRatio`
- `aria-hidden="true"` since it's decorative

## Integration

### CommandInput Changes
Two new optional props added:
- `trailingSlot: ReactNode` -- renders between the text input and submit button
- `externalValue: string` -- syncs input value from external source (voice transcription)

### CommandBar Changes
- Imports `useExecuteVoiceAction` and `VoiceMicButton`
- Passes `VoiceMicButton` as `trailingSlot` to `CommandInput`
- `handleAudioReady` callback: sends blob to voice API, fills transcription into input, auto-submits

## i18n Keys
Added under `commandBar.voice` in both `en.json` and `es.json`:
- `record` / `stop` / `processing` -- button labels
- `permissionDenied` / `permissionGuide` -- permission error messages
- `noAudioDetected` / `transcriptionFailed` -- error messages

## Tests

### use-voice-input.test.ts
- Mocks `navigator.mediaDevices.getUserMedia` and `MediaRecorder`
- Tests all state transitions (idle -> recording -> processing)
- Tests permission denied handling
- Tests minimum duration rejection
- Tests error clearing on retry

### VoiceMicButton.test.tsx
- Mocks `useVoiceInput` and `useAudioWaveform` hooks
- Tests button rendering and labels for each state
- Tests click handlers (start/stop recording)
- Tests processing disabled state
- Tests permission denied and error alerts
- Tests waveform visibility during recording
- Tests mobile tap target size (48x48)
- Tests pulse animation during recording

### AudioWaveform.test.tsx
- Mocks canvas 2D context
- Tests canvas rendering and dimensions
- Tests bar drawing for frequency data
- Tests accessibility attributes
- Tests custom className and dimensions

## Design Decisions
1. **Separate hooks over monolithic component**: Each concern (recording, waveform, API) is a separate hook for testability and reuse.
2. **Raw fetch for voice upload**: `aiApiFetch` enforces JSON content-type, but voice needs multipart/form-data. Using raw `fetch` with duplicated auth logic is the cleaner trade-off.
3. **Slot pattern for CommandInput**: Using `trailingSlot` avoids tight coupling between CommandInput and voice functionality, keeping the component composable.
4. **Auto-submit after transcription**: The transcribed text is both displayed in the input (for visibility) and auto-submitted (for efficiency), matching the user's expectation that speaking a command should execute it.
