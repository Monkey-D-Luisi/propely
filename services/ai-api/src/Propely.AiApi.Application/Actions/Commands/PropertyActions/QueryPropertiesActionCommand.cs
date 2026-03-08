// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AiApi.Application.Actions.Parameters;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Commands.PropertyActions;

/// <summary>
/// Command to query/search properties via natural language action.
/// Parameters contain filters extracted from the classified intent.
/// </summary>
public sealed record QueryPropertiesActionCommand(
    QueryPropertiesParameters Parameters,
    Guid TenantId,
    Guid AgentId) : IRequest<ActionResult>;
