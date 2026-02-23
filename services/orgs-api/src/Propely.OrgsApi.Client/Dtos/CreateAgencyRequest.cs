// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Client.Dtos;

/// <summary>
/// Request DTO for creating an agency.
/// </summary>
public sealed record CreateAgencyRequest(string Name, string Slug);
