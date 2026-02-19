# Pull Request Workflow

## Overview

This workflow handles the creation of pull requests for the current branch. It automates the process of pushing changes and creating a PR using the project template.

## Trigger Command

**Command:** `"pr"`

## Workflow Steps

### Step 1: Verify Branch State

```bash
git branch --show-current
git log main..HEAD --oneline
```

**Abort conditions:**
- Current branch is `main` → "Cannot create PR from main branch. Create a feature branch first."
- No commits ahead of main → "No changes to create PR for."

### Step 2: Gather PR Information

```bash
# Get full diff for understanding changes
git diff main..HEAD

# Check remote status
git status -sb
```

Also identify the linked GitHub Issue from the active task file metadata:

```bash
rg -n "^- GitHub Issue: #[0-9]+" docs/tasks/*.md
```

If the task has a GitHub issue, capture the issue number and carry it into the PR body using a closing keyword.

Analyze the changes to determine:
- **Affected areas**: Which services/apps are modified (ai-api, orgs-api, properties-api, publishing-api, contacts-api, appointments-api, web)
- **Change type**: feat, fix, docs, refactor, test, chore
- **Scope**: The feature area (auth, orgs, ai, infra, web, agent)

### Step 3: Push to Remote (if needed)

```bash
git push -u origin <branch-name>
```

### Step 4: Create Pull Request

Use `gh pr create` with the template from `.github/PULL_REQUEST_TEMPLATE.md`.

**IMPORTANT:** Fill ALL sections of the template:
- If the task has `- GitHub Issue: #NNN`, include `Closes #NNN` near the top of the PR body.
- Prefer `Closes #NNN` over plain references so issue auto-close is guaranteed on merge.

```bash
gh pr create --title "<type>(<scope>): <description>" --body "$(cat <<'EOF'
Closes #<issue-number>

# Summary
<Brief description of what this PR does and why>

## Scope / Changes
- <Change 1>
- <Change 2>

## How to Test
1. <Step 1>
2. <Step 2>

## Testing
- [ ] `dotnet build services/ai-api/Propely.AiApi.sln`
- [ ] `dotnet build services/orgs-api/Propely.OrgsApi.sln`
- [ ] `dotnet build services/properties-api/Propely.PropertiesApi.sln`
- [ ] `dotnet build services/contacts-api/Propely.ContactsApi.sln`
- [ ] `dotnet build services/appointments-api/Propely.AppointmentsApi.sln`
- [ ] `dotnet build services/publishing-api/Propely.PublishingApi.sln`
- [ ] `dotnet test services/ai-api/Propely.AiApi.sln`
- [ ] `dotnet test services/orgs-api/Propely.OrgsApi.sln`
- [ ] `dotnet test services/properties-api/Propely.PropertiesApi.sln`
- [ ] `dotnet test services/contacts-api/Propely.ContactsApi.sln`
- [ ] `dotnet test services/appointments-api/Propely.AppointmentsApi.sln`
- [ ] `dotnet test services/publishing-api/Propely.PublishingApi.sln`
- [ ] `cd apps/web && npm run build`
- [ ] Other (describe):

## Risks
- <Risk description or "None identified">

## Docs / Notes
- <Documentation notes or "N/A">

## Quality Gates
- [x] Code compiles and runs locally
- [x] Formatting/linting checks pass (if applicable)
- [x] Tests added/updated and passing
- [x] No secrets committed
- [x] Walkthrough updated (docs/walkthroughs)
EOF
)"
```

**Validation before submit:**
- Confirm PR body contains at least one auto-close keyword (`Closes #...`, `Fixes #...`, or `Resolves #...`) when a task issue exists.

### Step 5: Report Result

Output to user:
- PR URL
- PR title
- Target branch

## Title Format by Branch Type

| Branch Pattern | Title Format | Example |
|----------------|--------------|---------|
| `ft-NNNN-*` | `docs(<scope>): <description> (#ft-NNNN)` | `docs(agent): add pr workflow (#ft-0002)` |
| `feat/*` | `feat(<scope>): <description>` | `feat(auth): add OAuth2 support` |
| `fix/*` | `fix(<scope>): <description>` | `fix(api): resolve timeout issue` |
| `cr-NNNN-*` | `fix(<scope>): address PR review feedback (#cr-NNNN)` | `fix(web): address PR review (#cr-0003)` |
| `audit-NNNN-*` | `fix(security): <description> (#audit-NNNN)` | `fix(security): add rate limiting (#audit-0001)` |

## Filling the Template

### Summary
- One paragraph explaining the purpose of the changes
- Reference the task file if applicable (e.g., "Implements task #ft-0001")

### Scope / Changes
- List each logical change as a bullet point
- Be specific about files or components affected

### How to Test
- Provide step-by-step instructions to verify the changes work
- Include specific commands, URLs, or actions

### Testing Checklist
- Check boxes for tests that were actually run
- Leave unchecked for tests not applicable to this change

### Risks
- Describe any potential risks or breaking changes
- "None identified" if the change is low-risk

### Quality Gates
- Verify each gate before checking the box

## Error Handling

### PR Already Exists
If a PR already exists:
```bash
gh pr view --web
```
Report the existing PR URL to the user.

### Push Rejected
If push is rejected (e.g., branch behind):
1. Inform the user
2. Suggest: `git pull --rebase origin main`

## Related Documents

- [PR Template](../../.github/PULL_REQUEST_TEMPLATE.md)
- [Autonomous Workflow](autonomous-workflow.md)
- [Fast Track Workflow](fast-track-workflow.md)
