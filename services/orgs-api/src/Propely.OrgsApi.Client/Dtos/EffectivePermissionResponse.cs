// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Client.Dtos;

/// <summary>
/// Response DTO representing a user's effective permission with its source.
/// </summary>
public sealed record EffectivePermissionResponse(
    string Permission,
    bool Granted,
    string Source);
