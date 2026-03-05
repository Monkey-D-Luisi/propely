// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Commands.PropertyActions;

/// <summary>
/// Command to create a new property via natural language action.
/// Parameters are extracted from the classified intent and passed as a dictionary.
/// </summary>
public sealed record CreatePropertyActionCommand(
    Dictionary<string, object?> Parameters,
    Guid TenantId,
    Guid AgentId) : IRequest<ActionResult>;
