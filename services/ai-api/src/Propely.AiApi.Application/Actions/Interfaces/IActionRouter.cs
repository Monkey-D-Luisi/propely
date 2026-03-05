// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Interfaces;

/// <summary>
/// Routes a classified intent to the appropriate action handler for execution.
/// Maps ActionType to MediatR commands and dispatches them.
/// </summary>
public interface IActionRouter
{
    Task<ActionResult> RouteAsync(ClassifiedIntent intent, Guid tenantId, Guid agentId, CancellationToken ct = default);
}
