// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AiApi.Application.Actions.Parameters;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Commands.AppointmentActions;

/// <summary>
/// Command to reschedule an appointment via natural language action.
/// </summary>
public sealed record RescheduleAppointmentActionCommand(
    RescheduleAppointmentParameters Parameters,
    Guid TenantId,
    Guid AgentId) : IRequest<ActionResult>;
