// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.FeatureFlags.Interfaces;

public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(string flagName, CancellationToken cancellationToken = default);
}
