// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.OrgsApi.Application.FeatureFlags.Queries.GetFeatureFlags;

public sealed record GetFeatureFlagsQuery : IRequest<GetFeatureFlagsResult>;

public sealed record GetFeatureFlagsResult(IReadOnlyList<FeatureFlagDto> Flags);

public sealed record FeatureFlagDto(string Name, bool IsEnabled, string? Description, string Source);
