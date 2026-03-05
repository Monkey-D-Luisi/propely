// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Commands.PropertyActions;

/// <summary>
/// Command to update an existing property via natural language action.
/// Parameters contain the property identifier and the fields to update.
/// </summary>
public sealed record UpdatePropertyActionCommand(
    Dictionary<string, object?> Parameters,
    Guid TenantId,
    Guid AgentId) : IRequest<ActionResult>;
