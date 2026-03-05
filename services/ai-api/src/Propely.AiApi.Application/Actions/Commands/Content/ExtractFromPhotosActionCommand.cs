// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Commands.Content;

/// <summary>
/// Command to extract structured property data from photos via OpenAI vision API.
/// Parameters should contain "image_urls" key with an array of image URLs (max 10).
/// </summary>
public sealed record ExtractFromPhotosActionCommand(
    Dictionary<string, object?> Parameters,
    Guid TenantId,
    Guid AgentId) : IRequest<ActionResult>;
