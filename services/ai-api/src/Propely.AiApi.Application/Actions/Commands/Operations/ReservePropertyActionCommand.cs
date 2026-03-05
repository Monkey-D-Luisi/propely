// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Commands.Operations;

/// <summary>
/// Command to reserve a property for a potential buyer or renter via natural language action.
/// Parameters contain the property identifier and optional contact name.
/// </summary>
public sealed record ReservePropertyActionCommand(
    Dictionary<string, object?> Parameters,
    Guid TenantId,
    Guid AgentId) : IRequest<ActionResult>;
