// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Application.WorkItems.Interfaces;
using Propely.AiApi.Domain.WorkItems;
using Microsoft.EntityFrameworkCore;

namespace Propely.AiApi.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for WorkItem aggregate.
/// </summary>
public sealed class WorkItemRepository : IWorkItemRepository
{
    private readonly AppDbContext _context;

    public WorkItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(WorkItem workItem, CancellationToken cancellationToken = default)
    {
        await _context.WorkItems.AddAsync(workItem, cancellationToken);
    }

    public async Task<WorkItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.WorkItems
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public Task UpdateAsync(WorkItem workItem, CancellationToken cancellationToken = default)
    {
        _context.WorkItems.Update(workItem);
        return Task.CompletedTask;
    }
}
