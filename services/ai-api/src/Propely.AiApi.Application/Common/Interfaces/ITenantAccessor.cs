// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Application.Common.Interfaces;

/// <summary>
/// Provides access to the current tenant (organization) context.
/// Returns null when no tenant context is available (e.g., background jobs, system operations),
/// which allows bypassing tenant-scoped query filters.
/// Returns Guid.Empty when in an HTTP context but the tenant claim is missing/invalid (fail-closed).
/// </summary>
public interface ITenantAccessor
{
    Guid? GetCurrentOrgId();
}
