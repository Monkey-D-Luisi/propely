// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.PropertiesApi.Application.Properties.Commands.ReorderMedia;

public sealed record ReorderMediaCommand : IRequest
{
    public Guid PropertyId { get; init; }
    public Guid TenantId { get; init; }
    public IReadOnlyList<MediaOrderItem> Items { get; init; } = [];
}

public sealed record MediaOrderItem(Guid MediaId, int DisplayOrder);
