// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Organizations;
using Microsoft.EntityFrameworkCore;

namespace Propely.OrgsApi.Infrastructure.Persistence.Repositories;

public sealed class InvitationRepository : IInvitationRepository
{
    private readonly AppDbContext _context;

    public InvitationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Invitation invitation, CancellationToken cancellationToken = default)
    {
        await _context.Invitations.AddAsync(invitation, cancellationToken);
    }

    public async Task<Invitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.Invitations.FirstOrDefaultAsync(i => i.Token == token, cancellationToken);
    }

    public async Task CancelByOrgAsync(Guid orgId, CancellationToken cancellationToken = default)
    {
        await _context.Invitations
            .Where(i => i.OrganizationId == orgId && i.Status == InvitationStatus.Pending)
            .ExecuteUpdateAsync(s => s
                .SetProperty(i => i.IsDeleted, true)
                .SetProperty(i => i.DeletedAtUtc, DateTime.UtcNow), cancellationToken);
    }
}
