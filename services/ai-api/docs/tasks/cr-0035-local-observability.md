# Code Review: cr-0035-local-observability
## Metadata
- PR: #64
- PR Link: https://github.com/Monkey-D-Luisi/ai-api-template/pull/64
- Target Branch: main
- CI Status: PENDING
- Review Date: 2026-02-02

## Changed Files
- `docker-compose.yml`
- `QUICKSTART.md`
- `docs/tasks/0013-local-observability-dashboard.md`
- `docs/walkthroughs/0013-local-observability-dashboard.md`
- `docs/backlog/agent-ready-epic.md`

## Review Sources
- Review Comments: 0 (Pending checks)
- Reviews: 0 (Pending checks)
- Issue Comments: 0 (Pending checks)

## Comment Resolution Plan

### MUST_FIX
- [x] [Security Warning](https://github.com/Monkey-D-Luisi/ai-api-template/pull/64#discussion_r1938531742): Reviewer flagged `DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true`.
  - Action: Add comment `# SECURITY: Local dev only` to clarify intent.
- [x] [OTLP Config](https://github.com/Monkey-D-Luisi/ai-api-template/pull/64/comments...): Missing configuration.
  - Action: Add `OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317` guidance.
- [x] [Port Mapping](https://github.com/Monkey-D-Luisi/ai-api-template/pull/64/comments...): Incorrect OTLP port.
  - Action: Change to `127.0.0.1:4317:4317`.
- [x] [Security Binding](https://github.com/Monkey-D-Luisi/ai-api-template/pull/64/comments...): Missing `127.0.0.1` prefix.
  - Action: Bind all dashboard ports to localhost.
- [x] [Restart Policy](https://github.com/Monkey-D-Luisi/ai-api-template/pull/64/comments...): Missing `unless-stopped`.
  - Action: Add policy.
- [x] [Grammar](https://github.com/Monkey-D-Luisi/ai-api-template/pull/64/comments...): `pattern` -> `patterns` in tech stack.
  - Action: Fix pluralization.

### SHOULD_FIX
- [x] [Image Pinning](https://github.com/Monkey-D-Luisi/ai-api-template/pull/64/comments...): Using `latest`.
  - Action: Pin to `8.1.0-preview...`.

### SUGGESTION
- [ ] ...
