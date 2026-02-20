// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.FeatureFlags.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;

namespace Propely.OrgsApi.Infrastructure.Services;

public sealed class FeatureFlagService : IFeatureFlagService
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IFeatureFlagDefaults _defaults;

    public FeatureFlagService(
        IFeatureFlagRepository repository,
        IFeatureFlagDefaults defaults)
    {
        _repository = repository;
        _defaults = defaults;
    }

    public async Task<bool> IsEnabledAsync(string flagName, CancellationToken cancellationToken = default)
    {
        var configDefaults = _defaults.GetDefaults();
        if (!configDefaults.TryGetValue(flagName, out var configValue))
            throw new NotFoundException($"Feature flag '{flagName}' is not defined.");

        var dbFlag = await _repository.GetByNameAsync(flagName, cancellationToken);
        return dbFlag?.IsEnabled ?? configValue;
    }
}
