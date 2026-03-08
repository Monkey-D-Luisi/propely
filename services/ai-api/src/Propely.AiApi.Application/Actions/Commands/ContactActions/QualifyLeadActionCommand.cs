// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AiApi.Application.Actions.Parameters;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Commands.ContactActions;

/// <summary>
/// Command to qualify a lead (change status to Qualified) via natural language action.
/// </summary>
public sealed record QualifyLeadActionCommand(
    QualifyLeadParameters Parameters,
    Guid TenantId,
    Guid AgentId) : IRequest<ActionResult>;
