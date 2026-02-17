# Epic 005: Email System

## Overview

Build a production-ready email system with proper templating and a pluggable SMTP driver. Currently only a basic SmtpEmailService with hardcoded HTML for invitation emails exists (via Mailhog in dev). This epic adds a proper templating engine, production email provider support, and all transactional email types needed by other epics.

## Success Criteria

- Email templates use a proper templating engine (Razor or Liquid)
- Templates support i18n (EN + ES)
- Production SMTP driver configurable via environment variables
- Dev environment continues to use Mailhog
- All transactional email types implemented: welcome, password reset, email verification, invitation, role change
- Emails are well-formatted, responsive HTML
- Email sending is async (via MediatR notification or background job)

## Technology Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Templating | Razor (RazorLight) | Already in .NET ecosystem, strong typing, familiar syntax |
| Production provider | SendGrid or Postmark | Reliable transactional email with good .NET SDKs |
| Dev provider | Mailhog (existing) | Already configured on port 10025/18025 |

## Task List

> **Convention:** Each task's PR must include `Closes #<issue>` in the PR body to auto-close the GitHub Issue.

### Task 0039 - Email templating system (Razor/Liquid)
- **Status:** DONE
- **GitHub Issue:** #191
- **Dependencies:** None
- **File:** `docs/tasks/0039-email-templating.md`
- **Scope:** Add RazorLight or similar templating engine. Create base email layout (header, footer, branding). Refactor existing invitation email to use template. Create IEmailTemplateRenderer interface.
- **Old Issue:** #71

### Task 0040 - Production SMTP driver (SendGrid/Postmark/SES)
- **Status:** DONE
- **GitHub Issue:** #192
- **Dependencies:** None
- **File:** `docs/tasks/0040-production-smtp.md`
- **Scope:** Add configurable email provider (IEmailSender implementations). Support SendGrid and/or Postmark via environment variable switch. Keep Mailhog as default for development. Add health check for email connectivity.
- **Old Issue:** #72

### Task 0041 - Transactional email types (welcome, reset, invite, role change)
- **Status:** DONE
- **GitHub Issue:** #193
- **Dependencies:** 0039
- **File:** `docs/tasks/0041-transactional-emails.md`
- **Scope:** Create email templates for: welcome email (on registration), password reset link, email verification link, invitation (improve existing), role change notification. All templates in EN + ES.

## Progress Tracker

| Phase | Tasks | Done | Remaining |
|-------|-------|------|-----------|
| Email System | 0039-0041 | 3 | 0 |
| **Total** | **3** | **3** | **0** |

## Dependency Graph

```
0039 (Templating) ──► 0041 (Transactional emails)
0040 (SMTP driver) ── standalone
```
