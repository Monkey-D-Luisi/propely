# Walkthrough: 0054-mcp-server-endpoint

## Overview
Added MCP (Model Context Protocol) Streamable HTTP endpoint at `/mcp` in the ai-api service, exposing all 20 AI action tools to external MCP-compatible clients (Claude Desktop, Cursor, etc.).

## Changes Made

### New Files
- `services/ai-api/src/Propely.AiApi.Api/MCP/PropelyMcpTools.cs` — 20 `[McpServerTool]` methods (one per action type) with typed parameters and `[Description]` attributes for auto-generated JSON Schema
- `services/ai-api/src/Propely.AiApi.Infrastructure/MCP/McpToolHandler.cs` — Bridge between MCP tool calls and the existing ActionRouter pipeline
- `services/ai-api/tests/Propely.AiApi.UnitTests/Infrastructure/MCP/McpToolHandlerTests.cs` — 25 unit tests
- `services/ai-api/tests/Propely.AiApi.IntegrationTests/MCP/McpEndpointTests.cs` — 4 integration tests using official MCP .NET client

### Modified Files
- `services/ai-api/src/Propely.AiApi.Api/Program.cs` — AddMcpServer + MapMcp("/mcp")
- `services/ai-api/src/Propely.AiApi.Api/Propely.AiApi.Api.csproj` — ModelContextProtocol.AspNetCore 1.1.0
- `services/ai-api/src/Propely.AiApi.Infrastructure/DependencyInjection.cs` — McpToolHandler registration

## Architecture
```
MCP Client (Claude Desktop, Cursor, etc.)
  └─ POST /mcp (Streamable HTTP JSON-RPC)
       └─ PropelyMcpTools.[ToolMethod]()
            └─ McpToolHandler.HandleToolCallAsync()
                 └─ IToolSchemaRegistry.ResolveActionType()
                      └─ IActionRouter.RouteAsync()
                           └─ MediatR → Handler → SDK Client
```

## Test Results
- 25 unit tests for McpToolHandler (tool routing, unknown tool, error handling, all 20 ActionType mappings)
- 4 integration tests (tools/list returns 20, tools/call executes, unknown tool throws, all have descriptions)
