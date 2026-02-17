# Demo Video Recording Guide

## Overview

Record the demo in Spanish first, then use AI dubbing (e.g., ElevenLabs, HeyGen, Rask.ai) to produce the English version. Target duration: **6-8 minutes**.

---

## Pre-Recording Setup

### 1. Environment

```powershell
# Start the full stack
.\scripts\dev-up.ps1

# Wait for all services to be healthy, then seed demo data
.\scripts\seed-demo.ps1
```

After seeding, you'll have:

| User | Email | Role in Acme Corp | Role in Startup Labs |
|------|-------|-------------------|---------------------|
| Alice Johnson | alice@saastemplate.test | Owner | Admin |
| Bob Chen | bob@saastemplate.test | Admin | - |
| Carol Santos | carol@saastemplate.test | - | Owner |
| Dave Miller | dave@saastemplate.test | Member | - |
| Eve Park | eve@saastemplate.test | Member | - |

Password for all: `Demo123!`

### 2. Browser

- Use Chrome/Edge in incognito mode (clean state, no extensions visible)
- Set viewport to **1920x1080** (standard for screen recording)
- Zoom level: **100%**
- Close all other tabs
- Disable browser notifications

### 3. Screen Recorder

- OBS Studio (free) or similar
- Record at **1920x1080, 30fps**
- Capture only the browser window (not the entire screen)
- Use a good microphone; record audio and screen together
- Keep a glass of water nearby

### 4. Quick Checklist Before Recording

- [ ] Stack running (`localhost:3000` loads)
- [ ] Seed data populated (verify: login as Alice, see Acme Corp with members)
- [ ] MailHog open in a separate browser window (`localhost:18025`) for the invitation scene
- [ ] Browser history cleared (no autocomplete suggestions in URL bar)
- [ ] Notifications silenced on your OS

---

## Scene Script

### Scene 1: Landing Page (30s)

**Action:** Open `http://localhost:3000`

**What to show:**
- Hero section with tagline
- Scroll down slowly through features
- Show the pricing section
- Switch language EN <-> ES (language switcher in the header)

**Talking points:**
- "This is the landing page — fully responsive, with i18n built in"
- "Pricing section is connected to Stripe — the plans are real"

---

### Scene 2: Registration & Login (55s)

**Action:** Click "Get Started" or navigate to `/register`

**What to show:**
- Register a new user (use a fresh email like `demo@example.com`)
- Show form validation (try submitting empty, short password, etc.)
- After registration, show the email verification notice
- Open MailHog in a separate window, show the verification email arrived
- Alternatively: skip registration and login directly as Alice (`alice@saastemplate.test` / `Demo123!`)
- On the login page, point out the **OAuth buttons** (Google, GitHub) — mention "Social login with Google and GitHub is built in"
- Click the **"Forgot password?"** link briefly to show the recovery form exists, then go back

**Talking points:**
- "Registration includes email verification, password rules, and rate limiting"
- "In development, emails go to MailHog. In production, SendGrid"
- "Google and GitHub OAuth are fully integrated — callback handling, account linking, and conflict detection"

---

### Scene 3: Organizations (50s)

**Action:** After login, you land on the organizations list

**What to show:**
- Alice sees 3 organizations: Acme Corp, Startup Labs, Solo Project
- Click into "Acme Corp"
- Show the organization dashboard
- Point out the **notification bell** in the header — click it to show the notifications dropdown
- Go to Settings — show the org name editing
- Mention delete org capability (don't actually delete)

**Talking points:**
- "Multi-org support out of the box — each user can own or belong to multiple organizations"
- "Built-in notification system with real-time bell indicator"
- "Settings, deletion, and all org management is role-gated with RBAC"

---

### Scene 4: Team Members & Invitations (60s)

**Action:** Navigate to Members page within Acme Corp

**What to show:**
- Member list with roles: Alice (Owner), Bob (Admin), Dave (Member), Eve (Member)
- Show pending invitation for `pending@saastemplate.test`
- Click "Invite Member" — invite a new email
- Show MailHog — the invitation email arrived with a link
- Show the role change dropdown: change Dave from Member to Admin
- Mention the remove member button

**Talking points:**
- "Full invitation flow with email delivery"
- "RBAC with three roles: Owner, Admin, Member"
- "Owners can change roles, admins can invite, members can view"

---

### Scene 5: Work Items + AI Smart Fill (90s) ⭐ KEY SCENE

> This is the most important scene in the demo. AI Smart Fill is a major differentiator — take your time and show it thoroughly.

**Action:** Navigate to Work Items within Acme Corp

**What to show:**

1. **List view** — Show the work items table with columns: title, status, priority, type, due date, effort
2. **Click into an existing item** — Show the detail view with all fields populated
3. **Create a new work item** — Click "New Work Item"
4. **Type a natural language description** in the Smart Fill text area, something like:
   > "We need to implement a dark mode toggle in the app settings by end of March. It's a high priority feature that should take about a week of work."
5. **Click "Smart Fill"** — Pause and let the viewer see AI populate **all 7 fields** in real time:
   - **Title**: extracted from the description (e.g., "Implement dark mode toggle in app settings")
   - **Description**: a structured, expanded version of the input
   - **Status**: auto-set to "New"
   - **Priority**: "High" (extracted from "high priority")
   - **Type**: "Feature" (inferred from "implement")
   - **Due Date**: end of March (parsed from natural language)
   - **Estimated Effort**: "1 week" (extracted from "about a week")
6. **Show the result** — All fields filled in, ready to save
7. **Edit an existing work item** — Open one of the seeded work items, click Edit, and re-run Smart Fill to show it can re-analyze and update existing items
8. **Try a second example** (optional, if time allows) with a bug report:
   > "Critical login bug — users on Safari can't complete OAuth flow. Needs to be fixed today."
   - This should fill: Priority=Critical, Type=Bug, Due Date=today, Effort=half day

**Talking points:**
- "This is AI Smart Fill — type what you need in natural language, and the AI extracts structured data into 7 fields automatically"
- "It understands priorities, types, dates, and effort estimates from conversational text"
- "This is a production-ready pattern you can reuse for any structured form in your SaaS — onboarding, support tickets, project planning"
- "The AI API is a separate microservice with its own endpoint — it degrades gracefully if no API key is configured"

> **Note:** Ensure the OpenAI API key is configured in `.env` (`AIAPI_OpenAi__ApiKey`). Without it, Smart Fill won't work. Test it before recording.

---

### Scene 6: Billing & Stripe (45s)

**Action:** Navigate to Billing page within Acme Corp

**What to show:**
- Current plan status (Free mode since this is local dev)
- Show the subscription plans
- Explain that in production, this connects to Stripe Checkout
- Mention one-time payments support

**Talking points:**
- "Billing with Stripe is fully integrated — subscriptions and one-time payments"
- "The template runs in 'free' mode by default for development"
- "Switch to Stripe mode with one environment variable"

---

### Scene 7: Admin Panel (45s)

**Action:** Navigate to Admin section (in the top nav or sidebar)

**What to show:**
- **Feature Flags** page: show the flags (BetaFeatures=ON, DarkMode=ON, etc.)
- Toggle a flag on/off to demonstrate the UI
- **Audit Logs** page: show the logged events from the seed data
- Use the filter/search on audit logs
- Click Export to show the CSV export capability

**Talking points:**
- "Admin panel with feature flags — toggle features without redeploying"
- "Audit logs track every important action: who did what, when"
- "Filterable and exportable — ready for compliance"

---

### Scene 8: User Profile (25s)

**Action:** Click on profile (top right) or navigate to Profile page

**What to show:**
- Name editing
- Password change
- Account deletion button (mention it, don't click)
- **Briefly resize the browser window** to show responsive design (the layout adapts cleanly)

**Talking points:**
- "Profile management includes account deletion for GDPR compliance"
- "The entire UI is fully responsive — works on mobile, tablet, and desktop"

---

### Scene 9: Developer Experience (60s)

**Action:** Switch to the terminal/IDE

**What to show:**
- Quick tour of the project structure in the file tree
- Run `.\scripts\bootstrap.ps1` (or show it already running) — this checks prerequisites, creates `.env`, starts the full stack, waits for health checks, and opens the browser automatically
- Show the `.env.example` briefly
- Open the README.md and scroll through the competitor comparison table
- Open the Aspire Dashboard (`localhost:18888`) — show traces/logs

**Talking points:**
- "One command to go from clone to running — bootstrap checks prerequisites, configures everything, and opens the browser"
- "Clean Architecture + CQRS on the backend, Next.js App Router on the frontend"
- "OpenTelemetry for observability out of the box"
- "Full CI/CD with GitHub Actions, Terraform for GCP deployment"

---

### Scene 10: Closing (25s)

**Action:** Back to the landing page or show the README

**Talking points:**
- "Everything you've seen is included: auth, billing, teams, AI-powered forms, infra, CI/CD"
- "Plus what we didn't have time to show: multi-tenant data isolation with global query filters, forgot password flow, localized email templates in English and Spanish, responsive design across all pages"
- "The AI Smart Fill alone saves hours of manual data entry — and the pattern is reusable for any form"
- Mention where to buy and how to get started

---

## Tips for Recording

### Speaking

- Speak at a **moderate pace** — AI translation works better with clear speech and natural pauses
- **Pause for 1-2 seconds** between scenes — this gives the AI translation tool clean cut points
- Avoid slang or filler words ("eh", "bueno pues") — they translate poorly
- Use short, declarative sentences — easier for AI to translate accurately

### Navigation

- **Move the mouse slowly** — viewers need to follow your cursor
- **Wait 1 second** after clicking before speaking about what loaded — gives the viewer time to see the result
- If a page takes a moment to load, say "and here we can see..." while it loads
- **Don't scroll too fast** — the viewer needs to read what's on screen

### Editing

- Record in one continuous take if possible (fewer cuts = more natural)
- If you make a mistake, pause for 3 seconds, then repeat the section — you can cut it later
- Add a simple intro card (5s) and outro card (5s) in post-production

### AI Translation

- **ElevenLabs Dubbing** or **HeyGen** are good options for voice + lip sync
- **Rask.ai** is another popular choice for video translation
- Upload the Spanish video, select English as target language
- Review the output — AI may struggle with technical terms like "RBAC", "CSRF", "webhook" — these should stay in English in both versions
- If the tool allows a glossary, add: RBAC, CSRF, JWT, OAuth, Stripe, SendGrid, Terraform, Docker, CQRS, MediatR

### File Naming

- `demo-es.mp4` — Spanish version
- `demo-en.mp4` — English version (AI-translated)

---

## Post-Production Checklist

- [ ] Intro card with product name and tagline
- [ ] No personal data visible (check terminal, browser tabs, OS notifications)
- [ ] Audio levels consistent throughout
- [ ] No dead air longer than 3 seconds
- [ ] Outro card with purchase link / landing URL
- [ ] English version reviewed for translation accuracy
- [ ] Both versions uploaded to hosting (YouTube unlisted, Vimeo, or direct embed)
