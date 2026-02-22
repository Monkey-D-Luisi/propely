// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Permissions;

namespace Propely.OrgsApi.Application.Permissions.DTOs;

/// <summary>
/// Represents a user's effective permission with its source.
/// </summary>
public sealed record EffectivePermissionDto(
    Permission Permission,
    bool Granted,
    string Source);
