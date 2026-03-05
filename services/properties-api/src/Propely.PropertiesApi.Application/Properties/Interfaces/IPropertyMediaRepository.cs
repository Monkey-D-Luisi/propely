// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.Application.Properties.Interfaces;

public interface IPropertyMediaRepository
{
    Task<PropertyMedia?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PropertyMedia>> GetByPropertyIdAsync(Guid propertyId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<int> CountByPropertyIdAsync(Guid propertyId, Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(PropertyMedia media, CancellationToken cancellationToken = default);
    void Update(PropertyMedia media);
    void Delete(PropertyMedia media);
}
