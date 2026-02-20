// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.Billing.Interfaces;

public interface IEntitlementService
{
    Task<bool> CanCreateOrganizationAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> CanAddMemberAsync(Guid orgId, CancellationToken cancellationToken = default);
}
