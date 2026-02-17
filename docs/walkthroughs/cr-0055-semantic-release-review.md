# Walkthrough: CR-0055 — Semantic Release PR Review

- **Task**: `docs/tasks/cr-0055-semantic-release-review.md`
- **PR**: [#255](https://github.com/Monkey-D-Luisi/saas-template/pull/255)

## What Changed

1. **Added breaking change rule** — Custom `releaseRules` override the preset defaults. Added `{ "breaking": true, "release": "major" }` as the first rule to satisfy requirement R4.

2. **Added `@semantic-release/npm` plugin** — Without this prepare-step plugin, `package.json` version stays at `0.0.0`. Added with `npmPublish: false` to update the version field without publishing to npm (AC5).

3. **Fixed `persist-credentials`** — Changed from `false` to `true` so `@semantic-release/git` can push the release commit back to main using the checkout token.

4. **Gated release on CI** — Changed workflow trigger from `on: push` to `on: workflow_run` after CI succeeds, preventing releases when CI fails.

5. **Explicit plugin dependencies** — Added `@semantic-release/commit-analyzer`, `@semantic-release/release-notes-generator`, and `@semantic-release/github` to `devDependencies` for reproducible builds.

6. **Updated walkthrough** — Security section and decisions section now accurately reflect `persist-credentials: true` and `workflow_run` trigger.

## Rejected
- Task/epic status change suggestions — consistent with established project pattern (task 0050) where status = DONE with DoD items "pending first merge to main".
- Copilot's breaking rule placement (after `feat`) — already fixed correctly (before `feat`) per Gemini's suggestion.

## Validation
- YAML syntax verified
- `npm ci` passes with updated `package-lock.json`
- No code changes requiring build/test validation
