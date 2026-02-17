// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.AiApi.Application.Common.Interfaces;

namespace SaasTemplate.AiApi.Infrastructure.Persistence;

/// <summary>
/// Unit of work implementation that delegates to AppDbContext.
/// Domain events are automatically dispatched in SaveChangesAsync.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
