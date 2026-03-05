// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Commands.Operations;

/// <summary>
/// Command to reactivate an archived or withdrawn property via natural language action.
/// Parameters contain the property identifier.
/// </summary>
public sealed record ReactivatePropertyActionCommand(
    Dictionary<string, object?> Parameters,
    Guid TenantId,
    Guid AgentId) : IRequest<ActionResult>;
