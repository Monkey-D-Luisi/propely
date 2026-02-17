# Walkthrough: CR-0054 — GHCR Publish PR Review

- **Task**: `docs/tasks/cr-0054-ghcr-publish-review.md`
- **PR**: [#253](https://github.com/Monkey-D-Luisi/saas-template/pull/253)

## What Changed

1. **Fixed SHA tag source** — The `docker/metadata-action` SHA tag now explicitly uses `github.event.workflow_run.head_sha` instead of the default `github.sha`, preventing tag divergence when main advances between CI finish and publish start.

2. **Scan by digest** — Trivy now scans the exact image digest from the build step (`steps.build.outputs.digest`) instead of a tag reference, ensuring the scan targets the exact artifact that was pushed.

3. **DoD checklist honesty** — Unchecked "Workflow runs successfully" and "All three images published to GHCR" since these can only be verified after first merge to main.

## Rejected
- Gemini's claim that `trivy-action@0.28.0` is invalid — the version exists.
- Copilot's semver tag suggestion — deferred to task 0051 (semantic-release).

## Validation
- YAML syntax verified
- No code changes requiring build/test validation
