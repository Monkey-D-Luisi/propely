// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Application.Leads.Interfaces;
using Propely.ContactsApi.Domain.Leads;
using Microsoft.EntityFrameworkCore;

namespace Propely.ContactsApi.Infrastructure.Persistence.Repositories;

public sealed class LeadRepository : ILeadRepository
{
    private readonly AppDbContext _context;

    public LeadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Lead?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == id && l.TenantId == tenantId, cancellationToken);
    }

    public async Task AddAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        await _context.Leads.AddAsync(lead, cancellationToken);
    }

    public void Update(Lead lead)
    {
        _context.Leads.Update(lead);
    }

    public void Delete(Lead lead)
    {
        _context.Leads.Update(lead);
    }
}
