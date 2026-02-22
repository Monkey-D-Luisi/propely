// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Api.Dtos;

/// <summary>
/// Request body for setting a permission override.
/// </summary>
public sealed record SetPermissionOverrideRequest(bool Granted);
