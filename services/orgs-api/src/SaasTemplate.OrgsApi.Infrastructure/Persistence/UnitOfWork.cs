// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace SaasTemplate.OrgsApi.Infrastructure.Persistence;

/// <summary>
/// Unit of work implementation that delegates to AppDbContext.
/// Domain events are automatically dispatched in SaveChangesAsync.
/// Also sets the scoped audit context on the pooled DbContext instance.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(
        AppDbContext context,
        IAuditContext? auditContext = null,
        ILogger<AppDbContext>? logger = null)
    {
        _context = context;
        // Inject scoped audit context into the pooled DbContext so that
        // audit logging in SaveChangesAsync has access to the current user.
        _context.SetAuditContext(auditContext, logger);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await action();
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
