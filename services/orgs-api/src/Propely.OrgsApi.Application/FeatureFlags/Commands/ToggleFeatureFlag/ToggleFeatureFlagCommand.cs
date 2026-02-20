// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.OrgsApi.Application.FeatureFlags.Commands.ToggleFeatureFlag;

public sealed record ToggleFeatureFlagCommand(string Name, bool IsEnabled) : IRequest;
