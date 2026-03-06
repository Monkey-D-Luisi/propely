// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Domain.Appointments;
using Microsoft.EntityFrameworkCore;

namespace Propely.AppointmentsApi.Infrastructure.Persistence.Repositories;

public sealed class SyncOperationRepository : ISyncOperationRepository
{
    private readonly AppDbContext _context;

    public SyncOperationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SyncOperation>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SyncOperations
            .Where(s => s.Status == SyncOperationStatus.Pending)
            .OrderBy(s => s.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SyncOperation operation, CancellationToken cancellationToken = default)
    {
        await _context.SyncOperations.AddAsync(operation, cancellationToken);
    }

    public void Update(SyncOperation operation)
    {
        _context.SyncOperations.Update(operation);
    }
}
