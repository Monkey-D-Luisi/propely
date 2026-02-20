// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.Common.Auth;

/// <summary>
/// Shared JWT claim type constants used across Api and Infrastructure layers.
/// </summary>
public static class AuthClaimTypes
{
    public const string PasswordVersion = "pwd_ver";
    public const string SystemAdmin = "sys_admin";
    public const string OrgIds = "org_ids";
}
