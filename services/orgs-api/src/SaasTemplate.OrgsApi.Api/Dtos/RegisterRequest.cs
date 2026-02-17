// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace SaasTemplate.OrgsApi.Api.Dtos;

public sealed record RegisterRequest(string Email, string Password, string? Name, string? Locale = null);
