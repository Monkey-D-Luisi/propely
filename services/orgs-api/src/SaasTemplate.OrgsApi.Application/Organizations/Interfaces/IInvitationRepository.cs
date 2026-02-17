// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Domain.Organizations;

namespace SaasTemplate.OrgsApi.Application.Organizations.Interfaces;

public interface IInvitationRepository
{
    Task AddAsync(Invitation invitation, CancellationToken cancellationToken = default);
    Task<Invitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task CancelByOrgAsync(Guid orgId, CancellationToken cancellationToken = default);
}
