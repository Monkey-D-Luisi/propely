// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AiApi.Application.Actions.Parameters;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Commands.AppointmentActions;

/// <summary>
/// Command to book a property viewing via natural language action.
/// </summary>
public sealed record BookViewingActionCommand(
    BookViewingParameters Parameters,
    Guid TenantId,
    Guid AgentId) : IRequest<ActionResult>;
