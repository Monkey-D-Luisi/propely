// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Domain.Users;

namespace SaasTemplate.OrgsApi.Application.Users.Interfaces;

public interface IUserExternalLoginRepository
{
    Task AddAsync(UserExternalLogin login, CancellationToken cancellationToken = default);
    Task<UserExternalLogin?> GetByProviderAndExternalIdAsync(string provider, string externalId, CancellationToken cancellationToken = default);
    Task<UserExternalLogin?> GetByUserIdAndProviderAsync(Guid userId, string provider, CancellationToken cancellationToken = default);
}
