// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Agencies;

namespace Propely.OrgsApi.Application.Agencies.Interfaces;

public interface IAgencyRepository
{
    Task AddAsync(Agency agency, CancellationToken cancellationToken = default);
    Task<Agency?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Agency?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySlugAsync(string slug, Guid? excludeAgencyId = null, CancellationToken cancellationToken = default);
    Task<List<Agency>> ListForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
