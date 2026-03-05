# Task: 0029-frontend-command-bar

## Metadata
- ID: 0029
- Type: Standard
- Status: IN_PROGRESS
- Owner: Agent
- Created: 2026-03-05
- Related docs:
  - Walkthrough: `docs/walkthroughs/0029-frontend-command-bar.md`
  - Epic: `docs/backlog/epic-P3-ai-action-engine.md`

## Goal
Build a global command bar accessible via Ctrl+K / Cmd+K using `cmdk`. Users type natural language commands (e.g., "create a property at 123 Main St") and see results including success messages, error states with retry, and confirmation flows.

## Context
The command bar is the primary user-facing interface for the AI action engine. It provides a keyboard-shortcut-accessible overlay where agents can type natural language commands that are sent to `POST /v1/actions/execute` for intent classification and execution. The command bar supports recent command history (session-scoped), loading states, error handling with retry, and confirmation flows for actions that require user approval before execution.

## Scope
### In scope
- `cmdk`-based command bar overlay (Ctrl+K / Cmd+K)
- `useCommandBar` hook for open/close state, keyboard shortcuts, recent commands
- `useExecuteAction` hook wrapping `aiApiFetch` for action execution
- `CommandInput` component with text input and submit button
- `CommandResult` component for success/error/loading states
- `CommandHistory` component for recent commands (last 10, session-scoped)
- `ConfirmationPanel` component for actions needing user confirmation
- i18n translations (English and Spanish)
- Integration into root layout
- Vitest + React Testing Library tests

### Out of scope
- Voice input integration (separate task #0027)
- Persistent command history across sessions
- Action-specific result renderers
- Mobile-specific gestures

## Requirements
- R1: Command bar opens on Ctrl+K (Windows/Linux) or Cmd+K (macOS)
- R2: Command bar closes on Escape or backdrop click
- R3: Typing text and pressing Enter or clicking Execute sends command to AI API
- R4: Loading spinner shown during API call
- R5: Success message displayed with green check icon
- R6: Error message displayed with retry button
- R7: Confirmation panel shown when `needsConfirmation` is true in response
- R8: Recent commands stored in session (last 10, most recent first)
- R9: All text uses i18n translations (English and Spanish)
- R10: Desktop: centered modal (max-w-2xl). Mobile: full-width.

## Design Tokens
- Page background: `bg-surface`
- Card borders: `border-slate-200`
- Cards: `rounded-xl`
- Inputs/buttons: `rounded-lg`
- Primary button: `bg-primary-600 hover:bg-primary-600/90 text-white`
- Text: `text-slate-900` / `text-slate-600`

## Test Plan
- CommandBar: renders nothing when closed, renders dialog when open, keyboard shortcut, close behavior, submit flow, loading/success/error states, confirmation flow
- CommandInput: placeholder, submit on Enter, submit on click, disabled states, loading label
- CommandResult: loading spinner, error with retry, success message, null state
- CommandHistory: empty state, renders commands, calls onSelect on click
