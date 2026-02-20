// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.OrgsApi.Application.FeatureFlags.Interfaces;

namespace Propely.OrgsApi.Application.FeatureFlags.Queries.GetFeatureFlags;

public sealed class GetFeatureFlagsQueryHandler : IRequestHandler<GetFeatureFlagsQuery, GetFeatureFlagsResult>
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IFeatureFlagDefaults _defaults;

    public GetFeatureFlagsQueryHandler(
        IFeatureFlagRepository repository,
        IFeatureFlagDefaults defaults)
    {
        _repository = repository;
        _defaults = defaults;
    }

    public async Task<GetFeatureFlagsResult> Handle(GetFeatureFlagsQuery request, CancellationToken cancellationToken)
    {
        var configDefaults = _defaults.GetDefaults();
        var dbOverrides = await _repository.GetAllAsync(cancellationToken);
        var dbLookup = dbOverrides
            .GroupBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var flags = configDefaults
            .Select(kvp =>
            {
                var hasDbOverride = dbLookup.TryGetValue(kvp.Key, out var dbFlag);
                return new FeatureFlagDto(
                    kvp.Key,
                    hasDbOverride ? dbFlag!.IsEnabled : kvp.Value,
                    hasDbOverride ? dbFlag!.Description : null,
                    hasDbOverride ? "Database" : "Configuration");
            })
            .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new GetFeatureFlagsResult(flags);
    }
}
