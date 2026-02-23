// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Client.Dtos;

/// <summary>
/// Response DTO for the agency creation endpoint.
/// </summary>
public sealed record CreateAgencyResponse(Guid Id, string Name, string Slug);
