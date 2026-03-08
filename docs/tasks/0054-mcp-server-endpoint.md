# Task: 0054-mcp-server-endpoint

## Metadata
- ID: 0054
- Type: Standard
- Status: DOING
- Owner: Agent
- Created: 2026-03-08
- Epic: P8 — AI Provider Abstraction & MCP Server (Task 8.4)
- Related docs:
  - Walkthrough: `docs/walkthroughs/0054-mcp-server-endpoint.md`
  - Epic: `docs/backlog/epic-P8-ai-provider-abstraction.md`

## Goal
Mount an MCP (Model Context Protocol) Streamable HTTP endpoint at `/mcp` in the ai-api service, exposing all 20 action tools to external AI clients. Reuse the existing ActionRouter pipeline for execution.

## Acceptance Criteria
- [x] `ModelContextProtocol.AspNetCore` NuGet package added
- [x] `/mcp` endpoint responds to MCP protocol (tools/list, tools/call)
- [x] `tools/list` returns all 20 tool schemas
- [x] `tools/call` with valid tool name + arguments executes the action and returns result
- [x] `tools/call` with unknown tool name returns MCP error
- [x] Authentication enforced on the MCP endpoint
- [x] Tenant isolation: org_id extracted from ClaimsPrincipal
- [x] Coexists with existing REST endpoints
- [x] Unit tests for McpToolHandler
- [x] Integration test for MCP endpoint

## Implementation
See walkthrough: `docs/walkthroughs/0054-mcp-server-endpoint.md`
