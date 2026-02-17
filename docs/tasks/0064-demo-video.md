# Task: 0064 - Demo Video / Walkthrough

## Metadata
- ID: 0064
- Type: Documentation
- Status: PENDING
- Owner: Agent
- Created: 2026-02-07
- GitHub Issue: #216
- Epic: `docs/backlog/epic-012-demo-marketing.md`
- Old Issue: #53
- Milestone: v1.0

## Goal
Create a 2-3 minute demo video or animated walkthrough showing the template's key features in action.

## Context
A demo video is essential for marketing the SaaS template. It should show potential buyers what they'll get: the registration flow, organization management, notification system, language switching, feature flags, and admin capabilities.

## Scope
### In scope
- Script/storyboard for the demo
- Screen recording (or animated GIF walkthrough)
- Narration or text annotations
- Show: registration, login, org creation, member invitation, notifications, language switching, feature flags, org settings, user profile
- Post to docs or link from README
- GIFs/screenshots for README

### Out of scope
- Professional video production
- Marketing landing page
- Voice-over narration (text annotations sufficient)

## Requirements
- R1: Demo covers all major features (8+ features)
- R2: Duration: 2-3 minutes
- R3: Clear and professional looking
- R4: Usable in README and marketing materials
- R5: Individual screenshots/GIFs for each feature

## Acceptance Criteria
- AC1: Demo video/walkthrough exists
- AC2: Covers registration, org management, notifications, i18n, feature flags
- AC3: Screenshots/GIFs extracted for README
- AC4: Duration is 2-3 minutes

## Implementation Steps

1. **Write script/storyboard** listing each scene:
   - Scene 1: Registration form
   - Scene 2: Login and dashboard
   - Scene 3: Create organization
   - Scene 4: Invite member
   - Scene 5: Notification bell
   - Scene 6: Language switching (EN <-> ES)
   - Scene 7: Feature flags admin
   - Scene 8: Org settings
   - Scene 9: User profile
   - Scene 10: Admin dashboard overview

2. **Prepare demo environment** (run seed-dev script from task 0063)

3. **Record screen** using OBS, Loom, or similar tool

4. **Add text annotations** for each feature transition

5. **Export video** (MP4 for YouTube/Loom, GIF for README)

6. **Extract screenshots** for individual features

7. **Store assets** in `docs/demo/` directory

8. **Link from README** and marketing materials

## Files to Create

- `docs/demo/script.md` (storyboard)
- `docs/demo/` (video file or link)
- `docs/screenshots/` (individual feature screenshots)
- `docs/walkthroughs/0064-demo-video.md`

## Definition of Done Checklist
- [ ] Demo video/walkthrough created
- [ ] All major features shown
- [ ] Screenshots extracted
- [ ] Linked from README
- [ ] Walkthrough updated
