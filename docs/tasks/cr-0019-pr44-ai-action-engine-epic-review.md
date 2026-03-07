# Code Review: cr-0019-pr44-ai-action-engine-epic-review

## Metadata
- PR: #44 (`epic/P3-ai-action-engine` → `main`)
- CI Status: FAILURE (Web - Build & Test: function coverage 47.85% < 50%)
- Changed files: ~80+ files across ai-api, contacts-api SDK, appointments-api SDK, web, docs

## Section 1: Agent Review Findings (Phase A)

| # | Severity | Category | File/Area | Issue | Fix |
|---|----------|----------|-----------|-------|-----|
| A1 | MUST_FIX | Testing | CI (Web) | Function coverage 47.85% < 50% threshold | Add tests to raise coverage |

## Section 2: Review Comment Threads (Phase B)

### Inline Review Comments (13 total)

| # | Reviewer | Severity | Classification | File | Issue | Resolution |
|---|----------|----------|----------------|------|-------|------------|
| B1 | Copilot | MUST_FIX | MUST_FIX | BookViewingActionHandler.cs:71 | `DateTime.TryParse` without culture/UTC is timezone-dependent | Add `GetDateTimeUtc` to ParameterExtractor, use across handlers |
| B2 | Copilot | MUST_FIX | MUST_FIX | QueryAppointmentsActionHandler.cs:51 | Same `DateTime.TryParse` culture issue | Use new `GetDateTimeUtc` method |
| B3 | Copilot | MUST_FIX | MUST_FIX | RescheduleAppointmentActionHandler.cs:64 | Same `DateTime.TryParse` culture issue | Use new `GetDateTimeUtc` method |
| B4 | Copilot | SHOULD_FIX | SHOULD_FIX | RescheduleAppointmentActionHandler.cs:88 | No validation that end_time > start_time | Add validation check |
| B5 | Codex | MUST_FIX | MUST_FIX | QueryAppointmentsActionHandler.cs:65 | Sort key `"startTimeUtc"` not valid; API accepts `"start"` | Change to `"start"` |
| B6 | Codex | MUST_FIX | MUST_FIX | RescheduleAppointmentActionHandler.cs:99 | `IsAllDay` not preserved during reschedule | Add `IsAllDay = existing.IsAllDay` |
| B7 | Gemini | SUGGESTION | SUGGESTION | OpenAiIntentClassifier.cs:79 | `<tomorrow 10:00 ISO>` placeholder may confuse LLM | Replace with concrete ISO date |
| B8 | Copilot | MUST_FIX | MUST_FIX | docs/tasks/0045 | Status=DOING with unchecked DoD | Update to DONE with checked DoD |
| B9 | Copilot | MUST_FIX | MUST_FIX | docs/walkthroughs/0045 | Placeholder content | Fill with actual implementation details |
| B10 | Gemini | OUT_OF_SCOPE | OUT_OF_SCOPE | .stitch-html/ | Design inconsistencies (rounded-2xl vs rounded-xl) | Not in scope for ai-api epic |
| B11 | Copilot | OUT_OF_SCOPE | OUT_OF_SCOPE | AppSidebar.tsx | Material Symbols lack aria-hidden | Web accessibility, not ai-api scope |
| B12 | Copilot | OUT_OF_SCOPE | OUT_OF_SCOPE | OrgSettingsForm.tsx | Save button outside form | Web UI, not ai-api scope |
| B13 | Copilot | OUT_OF_SCOPE | OUT_OF_SCOPE | OrgSubNav.tsx | pathname.includes too loose | Web UI, not ai-api scope |

## Resolution Plan

### MUST_FIX (Blocking merge)
- [x] B1/B2/B3: Add `GetDateTimeUtc` to `ParameterExtractor` with `CultureInfo.InvariantCulture` + `DateTimeStyles.AssumeUniversal | AdjustToUniversal`, replace all inline `DateTime.TryParse` in appointment handlers
- [x] B5: Change `sortBy: "startTimeUtc"` → `sortBy: "start"` in QueryAppointmentsActionHandler
- [x] B6: Add `IsAllDay = existing.IsAllDay` to UpdateAppointmentClientRequest in RescheduleAppointmentActionHandler
- [x] B8: Update task 0045 Status → DONE, check all DoD boxes
- [x] B9: Fill walkthrough 0045 with actual implementation details
- [x] A1: Fix CI by raising web function coverage above 50%

### SHOULD_FIX
- [x] B4: Add end>start validation in RescheduleAppointmentActionHandler

### SUGGESTION
- [x] B7: Replace `<tomorrow 10:00 ISO>` with concrete date in system prompt example

### OUT_OF_SCOPE (Deferred)
- [ ] B10: Stitch HTML design inconsistencies — future design audit task
- [ ] B11: AppSidebar aria-hidden — future web accessibility task
- [ ] B12: OrgSettingsForm button placement — future web UI task
- [ ] B13: OrgSubNav pathname matching — future web UI task
