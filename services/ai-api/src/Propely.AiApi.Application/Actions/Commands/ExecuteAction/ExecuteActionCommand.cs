// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Commands.ExecuteAction;

/// <summary>
/// Command to execute an AI action from natural language text input.
/// Orchestrates intent classification, routing, and action execution.
/// </summary>
public sealed record ExecuteActionCommand(string Text, Guid TenantId, Guid AgentId) : IRequest<ActionResult>;
