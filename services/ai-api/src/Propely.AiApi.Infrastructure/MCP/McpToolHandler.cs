// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.Extensions.Logging;
using Propely.AiApi.Application.Actions.Interfaces;
using Propely.AiApi.Application.Actions.Tools;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Infrastructure.MCP;

/// <summary>
/// Bridges MCP tool calls to the existing AI Action Engine pipeline.
/// Resolves the tool name to an ActionType via <see cref="IToolSchemaRegistry"/>,
/// constructs a <see cref="ClassifiedIntent"/>, and delegates to <see cref="IActionRouter"/>.
/// </summary>
public sealed class McpToolHandler
{
    private readonly IToolSchemaRegistry _registry;
    private readonly IActionRouter _router;
    private readonly ILogger<McpToolHandler> _logger;

    public McpToolHandler(IToolSchemaRegistry registry, IActionRouter router, ILogger<McpToolHandler> logger)
    {
        _registry = registry;
        _router = router;
        _logger = logger;
    }

    /// <summary>
    /// Handles an MCP tools/call request by routing through the existing action pipeline.
    /// </summary>
    /// <param name="toolName">The MCP tool name (snake_case function name).</param>
    /// <param name="arguments">Tool arguments as a dictionary (may contain JsonElement values).</param>
    /// <param name="tenantId">The tenant (org) ID extracted from the authenticated user.</param>
    /// <param name="agentId">The agent (user) ID extracted from the authenticated user.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The action result from the handler pipeline.</returns>
    public async Task<ActionResult> HandleToolCallAsync(
        string toolName,
        Dictionary<string, object?>? arguments,
        Guid tenantId,
        Guid agentId,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "MCP tool call received: {ToolName} for tenant {TenantId} by agent {AgentId}",
            toolName, tenantId, agentId);

        // Resolve tool name to ActionType using the same registry as the classifier
        var actionType = _registry.ResolveActionType(toolName);

        if (actionType == ActionType.Unknown)
        {
            _logger.LogWarning("MCP tool call for unknown tool: {ToolName}", toolName);
            return ActionResult.Fail(
                [$"Unknown tool: '{toolName}'."],
                ActionType.Unknown,
                $"The tool '{toolName}' is not recognized. Use tools/list to see available tools.");
        }

        var parameters = arguments ?? new Dictionary<string, object?>();

        var intent = new ClassifiedIntent(
            ActionType: actionType,
            Confidence: 1.0, // MCP calls are explicit — no classification uncertainty
            RawFunctionName: toolName,
            Parameters: parameters);

        try
        {
            return await _router.RouteAsync(intent, tenantId, agentId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP tool execution failed for {ToolName}", toolName);
            return ActionResult.Fail(
                [$"An error occurred while executing tool '{toolName}'."],
                actionType,
                "The tool encountered an error during execution. Please try again.");
        }
    }
}
