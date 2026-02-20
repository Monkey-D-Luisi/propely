// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.Common.Interfaces;

/// <summary>
/// Provides the current user and correlation context for audit logging.
/// </summary>
public interface IAuditContext
{
    Guid? UserId { get; }
    string? CorrelationId { get; }
}
