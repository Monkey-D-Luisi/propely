// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AiApi.Application.Actions.Parameters;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Commands.Operations;

/// <summary>
/// Command to archive a property listing via natural language action.
/// Parameters contain the property identifier.
/// </summary>
public sealed record ArchivePropertyActionCommand(
    ArchivePropertyParameters Parameters,
    Guid TenantId,
    Guid AgentId) : IRequest<ActionResult>;
