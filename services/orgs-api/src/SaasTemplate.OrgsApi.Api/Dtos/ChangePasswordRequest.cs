// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace SaasTemplate.OrgsApi.Api.Dtos;

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
