// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.FeatureFlags;

namespace Propely.OrgsApi.Application.FeatureFlags.Interfaces;

public interface IFeatureFlagRepository
{
    Task<FeatureFlag?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<List<FeatureFlag>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(FeatureFlag flag, CancellationToken cancellationToken = default);
}
