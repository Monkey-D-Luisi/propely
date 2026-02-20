// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Client.Dtos;

/// <summary>
/// Envelope response from the /auth/me endpoint.
/// </summary>
public sealed record MeResponse(UserResponse User);
