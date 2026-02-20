// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.OrgsApi.Application.FeatureFlags.Queries.CheckFeatureFlag;

public sealed record CheckFeatureFlagQuery(string Name) : IRequest<CheckFeatureFlagResult>;

public sealed record CheckFeatureFlagResult(string Name, bool IsEnabled, string Source);
