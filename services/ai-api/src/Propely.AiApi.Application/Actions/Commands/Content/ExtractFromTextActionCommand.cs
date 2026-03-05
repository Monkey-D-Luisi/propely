// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Commands.Content;

/// <summary>
/// Command to extract structured property data from unstructured text via AI.
/// Parameters should contain "text" key with the raw text to parse.
/// </summary>
public sealed record ExtractFromTextActionCommand(
    Dictionary<string, object?> Parameters,
    Guid TenantId,
    Guid AgentId) : IRequest<ActionResult>;
