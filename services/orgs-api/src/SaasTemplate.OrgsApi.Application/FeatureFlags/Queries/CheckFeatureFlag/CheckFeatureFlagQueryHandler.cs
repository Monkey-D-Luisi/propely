// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using SaasTemplate.OrgsApi.Application.FeatureFlags.Interfaces;
using SaasTemplate.OrgsApi.Domain.Common.Exceptions;

namespace SaasTemplate.OrgsApi.Application.FeatureFlags.Queries.CheckFeatureFlag;

public sealed class CheckFeatureFlagQueryHandler : IRequestHandler<CheckFeatureFlagQuery, CheckFeatureFlagResult>
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IFeatureFlagDefaults _defaults;

    public CheckFeatureFlagQueryHandler(
        IFeatureFlagRepository repository,
        IFeatureFlagDefaults defaults)
    {
        _repository = repository;
        _defaults = defaults;
    }

    public async Task<CheckFeatureFlagResult> Handle(CheckFeatureFlagQuery request, CancellationToken cancellationToken)
    {
        var configDefaults = _defaults.GetDefaults();

        if (!configDefaults.TryGetValue(request.Name, out var configValue))
            throw new NotFoundException($"Feature flag '{request.Name}' is not defined.");

        var dbFlag = await _repository.GetByNameAsync(request.Name, cancellationToken);

        if (dbFlag is not null)
            return new CheckFeatureFlagResult(request.Name, dbFlag.IsEnabled, "Database");

        return new CheckFeatureFlagResult(request.Name, configValue, "Configuration");
    }
}
