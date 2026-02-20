// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Api.Dtos;

public sealed record RegisterRequest(string Email, string Password, string? Name, string? Locale = null);
