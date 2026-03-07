// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Domain.Appointments;
using Microsoft.EntityFrameworkCore;

namespace Propely.AppointmentsApi.Infrastructure.Persistence.Repositories;

public sealed class CalendarConnectionRepository : ICalendarConnectionRepository
{
    private readonly AppDbContext _context;

    public CalendarConnectionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CalendarConnection?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.CalendarConnections
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId, cancellationToken);
    }

    public async Task<CalendarConnection?> GetByAgentAndProviderAsync(
        Guid agentId, Guid tenantId, CalendarProvider provider, CancellationToken cancellationToken = default)
    {
        return await _context.CalendarConnections
            .FirstOrDefaultAsync(c =>
                c.AgentId == agentId &&
                c.TenantId == tenantId &&
                c.Provider == provider,
                cancellationToken);
    }

    public async Task<IReadOnlyList<CalendarConnection>> GetActiveByAgentAsync(
        Guid agentId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.CalendarConnections
            .Where(c =>
                c.AgentId == agentId &&
                c.TenantId == tenantId &&
                c.SyncState != CalendarSyncState.Disabled)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CalendarConnection>> ListActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CalendarConnections
            .Where(c => c.SyncState == CalendarSyncState.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CalendarConnection connection, CancellationToken cancellationToken = default)
    {
        await _context.CalendarConnections.AddAsync(connection, cancellationToken);
    }

    public void Update(CalendarConnection connection)
    {
        _context.CalendarConnections.Update(connection);
    }
}
