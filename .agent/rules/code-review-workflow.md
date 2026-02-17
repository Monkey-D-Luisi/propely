# Code Review Workflow

## Overview

This document defines the process for handling code reviews on pull requests. The agent follows this workflow when asked to address PR feedback or perform a code review pass.

## Trigger

The workflow is invoked when:
- User asks to review a PR
- User asks to address PR comments/feedback
- User shares a PR link for review

## Workflow Steps

### Operating Mode (Mandatory Autonomy)

During `code review`, the agent must execute this workflow **end-to-end without waiting for user confirmation between steps**.

- The agent must review all comments and the full diff.
- The agent must decide what applies vs. what does not apply.
- The agent must implement all applicable fixes immediately.
- The agent must document rejected/non-applicable items with rationale in the `cr-*` task file.
- The agent must run required validations and commit the fixes directly when checks pass.

Only stop and ask the user if there is a hard blocker that cannot be resolved locally (for example: missing credentials, external system outage, or contradictory repo rules).

### Step 0: Create the `cr-NNNN` Task + Walkthrough Files (Required)

Before making any code changes, create both files:

```
docs/tasks/cr-NNNN-<short-slug>.md
docs/walkthroughs/cr-NNNN-<short-slug>.md
```

The `cr-*` task must include:
- PR metadata (link/ID, target branch, CI status summary)
- Changed files list
- Review threads list (unresolved first)
- The "Comment Resolution Plan" checklist (grouped by classification)

The `cr-*` walkthrough must include:
- Task reference and PR reference
- What changed to address review feedback
- Commands run and validation results
- Any process deviations and corrective actions

#### How to Pick `NNNN` and `<short-slug>`

1. **Determine `NNNN`**: Scan `docs/tasks/` for existing `cr-*-*.md` files
   - Extract the numeric prefix from each
   - Use the next sequential number

2. **`<short-slug>`**: Must be kebab-case and short

#### Step 0.1: Hard Gate (Do Not Proceed Without Artifacts)

Before proceeding to Step 1, verify both files exist:

```bash
ls docs/tasks/cr-NNNN-<short-slug>.md
ls docs/walkthroughs/cr-NNNN-<short-slug>.md
```

If either file is missing, STOP and create it first.  
Do not start code edits, fixes, or comment replies before this gate passes.

### Step 1: Fetch PR Context

Connect to GitHub via `gh` CLI and fetch:

```bash
gh pr view <PR_NUMBER> --json title,body,files,state,statusCheckRollup
gh pr diff <PR_NUMBER>
```

#### Step 1.1: Fetch ALL Review Comments (CRITICAL)

**You MUST fetch from all three sources.** Different reviewers post comments in different locations.

```bash
# Source 1: Review Comments (inline comments on specific code lines)
gh api repos/<owner>/<repo>/pulls/<PR_NUMBER>/comments

# Source 2: Reviews (general review bodies and approval/rejection)
gh api repos/<owner>/<repo>/pulls/<PR_NUMBER>/reviews

# Source 3: Issue Comments (general PR discussion, not tied to code)
gh pr view <PR_NUMBER> --json comments
```

#### Step 1.2: Verify Comment Counts (MANDATORY)

After fetching, you MUST verify the total count from each source:

```bash
gh api repos/<owner>/<repo>/pulls/<PR_NUMBER>/comments --jq "length"
gh api repos/<owner>/<repo>/pulls/<PR_NUMBER>/comments --jq ".[] | {id: .id, user: .user.login, path: .path, line: .line, body: .body[0:100]}"
```

**Before proceeding to Step 2, confirm:**
- [ ] Total count of inline review comments: ___
- [ ] Total count of general reviews: ___
- [ ] Total count of issue comments: ___

#### Step 1.3: Handle Truncated Output (CRITICAL)

**If the Bash tool output shows `<persisted-output>` with a truncated message:**

1. The tool will provide a file path where the full output was saved
2. **You MUST use the Read tool** to read the COMPLETE output from that file
3. **DO NOT proceed** until you have verified you can see ALL comments

**IMPORTANT:** Tool output is truncated when it exceeds ~2KB. Always check for truncation indicators and read the full file when provided.

```bash
# If you see: "Output too large (48KB). Full output saved to: /path/to/file.txt"
# Then you MUST:
Read /path/to/file.txt
```

### Step 2: Analyze ALL Comments Critically

**CRITICAL: Process EVERY single comment. Do not stop after reading the first few.**

For EACH comment from ALL sources:

1. **Understand the claim**: What is the reviewer actually saying?
2. **Verify against project standards**: Does the suggestion align with documented standards?
3. **Assess correctness**: Is the reviewer's claim technically accurate?
4. **Make your own decision**: Reviewer suggestions are input, not instructions.

**Processing Order:**
1. Read ALL inline review comments (Source 1) - even if there are 10+
2. Read ALL general review bodies (Source 2) - check EACH review's body field for issues/suggestions
3. Read ALL issue comments (Source 3) - verify none contain actionable feedback

**Verification Checklist:**
- [ ] I have read and analyzed ALL inline comments (count matches Step 1.2)
- [ ] I have read ALL review bodies, not just summaries (count matches Step 1.2)
- [ ] I have checked all issue comments for actionable feedback
- [ ] I have not skipped any comments due to truncation or length

> Do not blindly follow reviewer suggestions. Automated reviewers may suggest changes that conflict with project-specific decisions. Always cross-reference with `.agent/rules/`, `.agent.md`, and existing architecture.

**Common Mistake:** Reading only the first issue from a review body that contains multiple numbered issues. Always read the ENTIRE body of each review to find ALL issues mentioned.

### Step 2.1: Mandatory Behavioral Parity Checks (AUTOMATIC)

After processing comments, run these checks against the full PR diff even if no reviewer mentioned them.  
If any check fails, classify it as **MUST_FIX** and address it in the same review pass.

1. **Redirect parity (`next` propagation)**
   - For auth entry points (`login`, `register`, OAuth buttons/callbacks), verify `next` is:
     - parsed consistently,
     - sanitized consistently,
     - applied consistently on success redirects.
   - Ensure credential flow and OAuth flow do not diverge unexpectedly.

2. **Locale source correctness for localized flows**
   - For localized emails/messages/redirects, verify locale is derived from explicit user/context input (route locale or request payload) where available.
   - Do not rely exclusively on `Accept-Language` when the active route locale is known.
   - Validate fallback behavior is deterministic (`en`/`es`).

3. **API-to-UI contract parity**
   - For each new/changed request DTO field in API, verify frontend request payloads are updated.
   - For each new frontend form field, verify backend contract/validation supports it.

4. **Test parity for behavior changes**
   - New/changed API behavior must have integration coverage for:
     - happy path,
     - at least one relevant error path.
   - New/changed form behavior must have frontend tests for:
     - success path,
     - key validation or redirect behavior.

**Parity Verification Checklist (must be recorded in the `cr-*` task file):**
- [ ] Redirect parity checked (`next` propagation and sanitization)
- [ ] Locale source correctness checked (explicit locale + fallback)
- [ ] API/UI contract parity checked (fields and payloads)
- [ ] Test parity checked (happy + error/validation paths)

### Step 3: Classify Review Threads

| Classification | Criteria | Action Required |
|----------------|----------|-----------------|
| **MUST_FIX** | Blocking merge, correctness issue, security vulnerability | Must address before merge |
| **SHOULD_FIX** | Maintainability, clarity, minor bug risk | Implement if low-risk and within scope |
| **SUGGESTION** | Style, nit, personal preference | Implement quickly or respond with rationale |
| **QUESTION** | Needs clarification | Respond with explanation |
| **OUT_OF_SCOPE** | Conflicts with task scope/roadmap | Explain why and defer to future task |

### Step 4: Produce the Comment Resolution Plan

Create a Markdown checklist in the task file grouped by classification.

### Step 5: Execute

1. **MUST_FIX**: Implement all items. These are blocking.
2. **SHOULD_FIX**: Implement when low-risk and within scope.
3. **SUGGESTION**: Implement quickly if trivial, or respond with rationale.
4. **QUESTION**: Respond with clear explanation.
5. **OUT_OF_SCOPE**: Respond explaining why and reference future task if applicable.

**Autonomous application policy (mandatory):**
- Do not ask the user for permission to apply an item that is technically valid and within scope.
- Decide and act in the same pass: classify -> implement -> validate -> document.
- For non-applicable comments, add explicit rationale in `docs/tasks/cr-NNNN-<short-slug>.md` and continue.

#### Rules

- **Never mark a thread as resolved without addressing it**
- **Keep changes minimal** and aligned with current task/roadmap
- **Avoid unrelated refactors**
- **Update tests** if code changes affect behavior
- **Update walkthrough** with changes made during review

### Step 6: Commit and Update

```bash
git add <specific-files>
git commit -m "fix(scope): address PR review feedback (#cr-NNNN)"
git push
```

**Commit policy (mandatory):**
- After validations pass, create the commit directly without asking "should I commit?".
- If fixes span independent concerns, create multiple small commits; otherwise use one atomic commit.
- If validation fails, keep iterating until fixed or until a real blocker is identified and documented.

Before committing, verify the artifact pair is included:
- [ ] `docs/tasks/cr-NNNN-<short-slug>.md` exists and is updated
- [ ] `docs/walkthroughs/cr-NNNN-<short-slug>.md` exists and is updated
- [ ] Both files are included in `git status`

### Step 7: Reply to Comments

For each addressed comment:
- Reply on GitHub indicating the fix
- Reference the commit if helpful
- Mark as resolved only after reply

## Related Documents

- [Autonomous Workflow](autonomous-workflow.md)
- [Task Template](../templates/task-template.md)
- [PR Review Template](../templates/pr-review-template.md)
