// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Application.Properties.Interfaces;
using Propely.PropertiesApi.Domain.Properties;
using Microsoft.EntityFrameworkCore;

namespace Propely.PropertiesApi.Infrastructure.Persistence.Repositories;

public sealed class PropertyRepository : IPropertyRepository
{
    private readonly AppDbContext _context;

    public PropertyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Property?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, cancellationToken);
    }

    public async Task AddAsync(Property property, CancellationToken cancellationToken = default)
    {
        await _context.Properties.AddAsync(property, cancellationToken);
    }

    public void Update(Property property)
    {
        _context.Properties.Update(property);
    }

    public void Delete(Property property)
    {
        _context.Properties.Update(property);
    }
}
