# Code Review: cr-0014-pr37-permission-ui-e2e-review

## PR Metadata
- **PR:** #37 — feat(web,orgs): Permission Management UI & E2E Tests (#0012, #0013)
- **Branch:** `feat/p1-permission-ui-e2e` → `main`
- **CI Status:** Queued (CI pipeline running)
- **Files Changed:** 30

## Changed Files
- apps/web/e2e/permission-management.spec.ts (new)
- apps/web/messages/en.json, es.json (i18n additions)
- apps/web/src/app/[locale]/orgs/[orgId]/permissions/page.tsx (new)
- apps/web/src/app/[locale]/orgs/[orgId]/permissions/[userId]/page.tsx (new)
- apps/web/src/app/[locale]/orgs/[orgId]/permissions/[userId]/PermissionDetailPageClient.tsx (new)
- apps/web/src/components/orgs/MembersManager.tsx (modified)
- apps/web/src/components/permissions/*.tsx (6 new components)
- apps/web/src/components/permissions/__tests__/*.test.tsx (5 new test files)
- apps/web/src/components/ui/icons.tsx (ShieldCheckIcon added)
- apps/web/src/hooks/permissions.ts (new)
- apps/web/src/lib/schemas.ts (EffectivePermission schema added)
- docs/backlog/epic-P1-agency-permissions.md (status updates)
- docs/tasks/0012-*, 0013-* (new task/walkthrough docs)
- services/orgs-api/tests/.../Permissions/*.cs (3 new integration test files)

---

## Section 1: Agent Review Findings

### F1 — MUST_FIX: `canEdit` derived from target user's role, not viewer's role
- **File:** `PermissionDetailPanel.tsx:711`
- **Category:** Security
- **Description:** `canEdit = isManager(userRole) || isOwner` uses the target user's role. When an admin views an agent, `isManager('agent')` returns false, disabling toggles for the admin. Conversely, when an agent navigates to an admin's permissions page, `isManager('admin')` returns true, enabling toggles for the unauthorized agent.
- **Fix:** Pass viewer role info into the component. The parent (`PermissionDetailPageClient`) already fetches all members via `useMembers`. Find the current user among them and pass `canManage` based on the viewer's role.

### F2 — MUST_FIX: "Manage" links shown to all users in PermissionMembersList
- **File:** `PermissionMembersList.tsx:138`
- **Category:** Security
- **Description:** The `MemberRow` component always renders a "Manage" link regardless of the viewer's role. Non-managers can click through to the detail page. Should be conditionally rendered based on `canManage`.
- **Fix:** Pass `canManage` to `MemberRow` and conditionally render the Manage link.

### F3 — SHOULD_FIX: Cancel button not disabled during processing in OverrideConfirmDialog
- **File:** `OverrideConfirmDialog.tsx:62`
- **Category:** UX
- **Description:** The Cancel button uses `if (!isProcessing) onCancel()` guard but is not visually disabled, which is inconsistent with the confirm button that uses `disabled={isProcessing}`.
- **Fix:** Add `disabled={isProcessing}` and disabled styling to the Cancel button.

### F4 — SHOULD_FIX: Test name mismatch in CrossTenantPermissionTests
- **File:** `CrossTenantPermissionTests.cs:114`
- **Category:** Code Quality
- **Description:** `GetPermissions_ForNonMemberInOwnOrg_ShouldReturnNotFound` asserts `BadRequest`, not `NotFound`.
- **Fix:** Rename to `GetPermissions_ForNonMemberInOwnOrg_ShouldReturnBadRequest`.

### F5 — SHOULD_FIX: Unused variable `orgAId` in CrossTenantPermissionTests
- **File:** `CrossTenantPermissionTests.cs:78,83,98`
- **Category:** Code Quality
- **Description:** `orgAId` is assigned but unused. May cause compiler warnings.
- **Fix:** Use discard (`_ = await CreateOrgAsync(...)`) where the return value is not needed.

### F6 — SHOULD_FIX: Missing assertion after grant override in PermissionLifecycleTests
- **File:** `PermissionLifecycleTests.cs:170-172`
- **Category:** Testing
- **Description:** The `RemoveOverride_AgentLosesGrantedPermission_ShouldReturnToRoleDefault` test does not assert the grant override succeeded before testing the remove.
- **Fix:** Capture the PUT response and assert `HttpStatusCode.OK`.

### F7 — SHOULD_FIX: Walkthrough 0012 mostly TBD stubs
- **File:** `docs/walkthroughs/0012-permission-management-ui.md`
- **Category:** Documentation
- **Description:** The walkthrough has TBD stubs for summary, decisions, implementation notes, commands, files changed, and tests sections, despite the task being marked DONE.
- **Fix:** Fill in the walkthrough with actual implementation details.

### F8 — SHOULD_FIX: Walkthrough 0013 scenario list mismatch
- **File:** `docs/walkthroughs/0013-e2e-permission-tests.md:38`
- **Category:** Documentation
- **Description:** Lists "Deny override on admin" but the actual test denies override on an agent's LeadsManage permission.
- **Fix:** Update scenario list to match implemented tests.

### F9 — NIT: SVG icons defined inline in PermissionDetailPanel
- **File:** `PermissionDetailPanel.tsx:100`
- **Category:** Code Quality
- **Description:** 6 SVG icon components (BuildingIcon, UsersIcon, etc.) are defined locally instead of in the shared `icons.tsx` file.
- **Fix:** Move to `icons.tsx` for reusability. (Deferred — low value, these icons are specific to this component, and moving them risks breaking existing tests)

### F10 — NIT: Hard-coded English strings in Playwright E2E spec
- **File:** `apps/web/e2e/permission-management.spec.ts`
- **Category:** Code Quality
- **Description:** Test uses hardcoded English strings. Could be brittle under i18n changes.
- **Fix:** FALSE_POSITIVE — E2E tests in this project consistently use English strings with no locale forcing needed; this is an established pattern across all existing E2E specs.

---

## Section 2: Review Comment Threads

### Gemini Comments
| # | Comment | Classification |
|---|---------|---------------|
| G1 | canEdit from target role (line 353) | MUST_FIX (= F1) |
| G2 | Fetching 100 members for single user (line 22) | OUT_OF_SCOPE |
| G3 | Manage links shown to all users (line 138) | MUST_FIX (= F2) |
| G4 | E2E test timeout 90s (line 6) | FALSE_POSITIVE |
| G5 | Cancel button not disabled (line 62) | SHOULD_FIX (= F3) |
| G6 | SVG icons inline (line 100) | NIT (= F9) |
| G7 | No optimistic update (line 219) | OUT_OF_SCOPE |

### Copilot Comments
| # | Comment | Classification |
|---|---------|---------------|
| C1 | canEdit from target role (line 140) | MUST_FIX (= F1) |
| C2 | sr-only label hardcoded English (line 32) | SHOULD_FIX |
| C3 | Unused orgAId (line 78, 83) | SHOULD_FIX (= F5) |
| C4 | Test name mismatch (line 114, 125) | SHOULD_FIX (= F4) |
| C5 | Missing assertion after grant (line 172) | SHOULD_FIX (= F6) |
| C6 | Duplicated test helpers (line 36, 44, 63) | OUT_OF_SCOPE |
| C7 | Walkthrough 0012 TBD (line 10, 21, 34) | SHOULD_FIX (= F7) |
| C8 | Walkthrough 0013 scenario mismatch (line 38) | SHOULD_FIX (= F8) |
| C9 | Hardcoded English in E2E (line 19, 24, 32) | FALSE_POSITIVE (= F10) |
| C10 | Zod schemas use unconstrained string (line 465) | OUT_OF_SCOPE |

---

## Resolution Plan

### MUST_FIX
- [x] F1/G1/C1: Fix `canEdit` — derive from current user's role, not target's role
- [x] F2/G3: Conditionally render Manage links based on `canManage`

### SHOULD_FIX
- [x] F3/G5: Disable Cancel button during processing
- [x] F4/C4: Rename test method to match assertion
- [x] F5/C3: Replace unused `orgAId` with discard
- [x] F6/C5: Add assertion after grant override
- [x] F7/C7: Fill in walkthrough 0012
- [x] F8/C8: Fix walkthrough 0013 scenario list
- [x] C2: Add translated sr-only label to PermissionToggle

### OUT_OF_SCOPE
- G2: Fetching 100 members — current codebase pattern, dedicated endpoint doesn't exist yet
- G7: Optimistic updates — would require significant hook refactor, tracked as follow-up
- C6: Duplicated test helpers — valid observation but refactoring test infra is out of PR scope
- C10: Zod enum types — API may add new permission names, string is intentionally flexible

### FALSE_POSITIVE
- G4: 90s timeout — standard for E2E tests that create orgs and navigate
- F10/C9: Hardcoded English — consistent with all existing E2E specs in the project

### NIT (Deferred)
- F9/G6: Inline icons — low value move, these are specific to PermissionDetailPanel
