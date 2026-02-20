// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.PublishingApi.Api.Services;

public interface ICorrelationIdAccessor
{
    string? CorrelationId { get; }
}

public sealed class CorrelationIdAccessor : ICorrelationIdAccessor
{
    public string? CorrelationId { get; set; }
}
