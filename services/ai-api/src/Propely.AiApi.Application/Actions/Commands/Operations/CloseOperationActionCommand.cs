// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AiApi.Application.Actions.Parameters;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Commands.Operations;

/// <summary>
/// Command to close a real estate operation (sale or rental) via natural language action.
/// Parameters contain the property identifier and optional operation type for status inference.
/// </summary>
public sealed record CloseOperationActionCommand(
    CloseOperationParameters Parameters,
    Guid TenantId,
    Guid AgentId) : IRequest<ActionResult>;
