// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Client.Dtos;

/// <summary>
/// Request DTO for setting a permission override.
/// </summary>
public sealed record SetPermissionOverrideRequest(bool Granted);
