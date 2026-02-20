// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Users.Interfaces;
using Propely.OrgsApi.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Propely.OrgsApi.Infrastructure.Persistence.Repositories;

public sealed class UserExternalLoginRepository : IUserExternalLoginRepository
{
    private readonly AppDbContext _context;

    public UserExternalLoginRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(UserExternalLogin login, CancellationToken cancellationToken = default)
    {
        await _context.UserExternalLogins.AddAsync(login, cancellationToken);
    }

    public async Task<UserExternalLogin?> GetByProviderAndExternalIdAsync(string provider, string externalId, CancellationToken cancellationToken = default)
    {
        var normalizedProvider = provider.Trim().ToLowerInvariant();
        var normalizedExternalId = externalId.Trim();

        return await _context.UserExternalLogins
            .FirstOrDefaultAsync(
                x => x.Provider == normalizedProvider && x.ExternalId == normalizedExternalId,
                cancellationToken);
    }

    public async Task<UserExternalLogin?> GetByUserIdAndProviderAsync(Guid userId, string provider, CancellationToken cancellationToken = default)
    {
        var normalizedProvider = provider.Trim().ToLowerInvariant();

        return await _context.UserExternalLogins
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Provider == normalizedProvider, cancellationToken);
    }
}
