// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AiApi.Application.Actions.Parameters;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Commands.Content;

/// <summary>
/// Command to generate marketing copy for a property listing in multiple languages.
/// Parameters may contain "property_data" (text description of the property),
/// "tone" (professional/luxury/casual/concise), and "languages" (array of language codes).
/// </summary>
public sealed record GenerateCopyActionCommand(
    GenerateCopyParameters Parameters,
    Guid TenantId,
    Guid AgentId) : IRequest<ActionResult>;
