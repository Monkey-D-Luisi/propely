// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Application.Properties.Interfaces;
using Propely.PropertiesApi.Domain.Properties;
using Microsoft.EntityFrameworkCore;

namespace Propely.PropertiesApi.Infrastructure.Persistence.Repositories;

public sealed class PropertyMediaRepository : IPropertyMediaRepository
{
    private readonly AppDbContext _context;

    public PropertyMediaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PropertyMedia?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.PropertyMedia
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == tenantId, cancellationToken);
    }

    public async Task<IReadOnlyList<PropertyMedia>> GetByPropertyIdAsync(Guid propertyId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.PropertyMedia
            .Where(m => m.PropertyId == propertyId && m.TenantId == tenantId)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByPropertyIdAsync(Guid propertyId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.PropertyMedia
            .CountAsync(m => m.PropertyId == propertyId && m.TenantId == tenantId, cancellationToken);
    }

    public async Task AddAsync(PropertyMedia media, CancellationToken cancellationToken = default)
    {
        await _context.PropertyMedia.AddAsync(media, cancellationToken);
    }

    public void Update(PropertyMedia media)
    {
        _context.PropertyMedia.Update(media);
    }

    public void Delete(PropertyMedia media)
    {
        _context.PropertyMedia.Remove(media);
    }
}
