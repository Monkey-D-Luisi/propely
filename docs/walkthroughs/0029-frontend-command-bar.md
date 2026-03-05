# Walkthrough: 0029-frontend-command-bar

## Overview
This walkthrough documents the implementation of the global command bar for the Propely web application. The command bar is accessible via Ctrl+K / Cmd+K and allows users to type natural language commands that are sent to the AI API for execution. It uses the `cmdk` library for the command palette UI pattern.

## File Structure

```
apps/web/src/
  hooks/
    use-command-bar.ts              # Open/close state, keyboard shortcut, recent commands
    use-execute-action.ts           # AI API action execution wrapper
  components/command-bar/
    CommandBar.tsx                   # Main overlay component (cmdk-based)
    CommandInput.tsx                 # Text input with search icon and submit button
    CommandResult.tsx                # Success/error/loading result display
    CommandHistory.tsx               # Recent commands list (session-scoped)
    ConfirmationPanel.tsx            # Confirmation dialog for actions needing approval
    __tests__/
      CommandBar.test.tsx            # Integration tests for main component
      CommandInput.test.tsx          # Unit tests for input component
      CommandResult.test.tsx         # Unit tests for result display
      CommandHistory.test.tsx        # Unit tests for history component
  app/[locale]/layout.tsx           # Root layout (CommandBar added as sibling)
messages/
  en.json                           # English translations (commandBar section)
  es.json                           # Spanish translations (commandBar section)
```

## Key Design Decisions

### 1. cmdk for Command Palette
We use `cmdk` v1.x as the foundation for the command bar. It provides accessible keyboard navigation, filtering, and grouping out of the box. We set `shouldFilter={false}` because our commands are executed via API rather than filtered locally.

### 2. Session-Scoped Recent Commands
Recent commands (last 10) are stored in React state within the `useCommandBar` hook. They persist for the browser session but do not survive page refresh. This is intentional -- persisting to localStorage or a backend would be a future enhancement.

### 3. Confirmation Flow
When the AI API response includes `needsConfirmation: true`, the command bar shows a `ConfirmationPanel` with extracted parameters. The user can confirm (which calls `POST /v1/actions/confirm`) or cancel. During confirmation, the text input is disabled to prevent new commands.

### 4. Error Handling with Retry
On error, the command bar shows the error message and a "Try again" button that re-executes the last command. This avoids forcing the user to retype their command.

### 5. Layout Integration
The `CommandBar` is added to the root locale layout (`app/[locale]/layout.tsx`) as a sibling to the main content div, inside the `<Providers>` wrapper. This ensures it has access to all context providers (i18n, toast, feature flags) while being rendered outside the main content flow.

## Hook Details

### useCommandBar
- `isOpen` / `open` / `close` / `toggle`: Boolean state with action functions
- `recentCommands`: Array of strings (max 10, most recent first, deduplicated)
- `addRecentCommand(command)`: Adds a command to the front of the list
- Keyboard shortcut: `Ctrl+K` / `Cmd+K` registered via `useEffect` on `document`

### useExecuteAction
- `execute(text)`: Sends `POST /v1/actions/execute` with `{ text }` body
- `confirm(confirmationId)`: Sends `POST /v1/actions/confirm` with `{ confirmationId }` body
- `isLoading` / `result` / `error`: State tracking for the API call
- `reset()`: Clears all state back to initial
- Uses `ensureCsrfToken()` for CSRF protection and `aiApiFetch` for Bearer token + X-Org-Id headers

## Component Details

### CommandBar (main component)
- Renders a fixed overlay with backdrop (`bg-slate-900/40`)
- Centered modal with `max-w-2xl` on desktop
- Uses `cmdk` `<Command>` as the root, `<Command.List>` for content
- Conditionally shows: result, confirmation panel, or recent history
- Closes on Escape key or backdrop click

### CommandInput
- Search icon + text input + submit button
- Submit on Enter or button click
- Shows "Processing..." during loading
- Supports `trailingSlot` for voice mic integration
- Supports `externalValue` for voice transcription injection

### CommandResult
- Loading: spinner + "Processing..." text
- Error: red icon + error message + "Try again" button
- Success: green check icon + "Done" + API message
- Returns null when `needsConfirmation` is true (handled by ConfirmationPanel)

### CommandHistory
- Heading "Recent commands"
- Empty state: "No recent commands"
- List of clickable recent commands with clock icon
- `onSelect` callback re-executes the selected command

### ConfirmationPanel
- "Please confirm" heading + API message
- Extracted parameters displayed in a bordered card
- Confirm and Cancel buttons
- Confirm sends `POST /v1/actions/confirm`

## i18n Keys
All user-facing strings use `useTranslations('commandBar')`:
- `placeholder`, `submit`, `loading`, `success`, `error`, `retry`
- `confirm`, `cancel`, `recentCommands`, `noRecentCommands`
- `openShortcut`, `close`, `needsConfirmation`, `noSpeechDetected`
- `voice.*` sub-keys for voice input integration

## Testing Strategy
Tests use Vitest + React Testing Library with the project's `renderWithProviders` utility (includes `NextIntlClientProvider` and `ToastProvider`). Hooks are mocked at the module level with `vi.mock()` to isolate component behavior.
