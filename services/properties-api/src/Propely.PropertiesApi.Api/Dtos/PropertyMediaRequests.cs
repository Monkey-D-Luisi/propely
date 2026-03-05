// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.PropertiesApi.Api.Dtos;

public sealed record ReorderRequest
{
    public IReadOnlyList<ReorderItemRequest> Items { get; init; } = [];
}

public sealed record ReorderItemRequest
{
    public Guid MediaId { get; init; }
    public int DisplayOrder { get; init; }
}
