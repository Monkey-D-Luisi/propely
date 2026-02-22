// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.AspNetCore.Authorization;
using Propely.OrgsApi.Domain.Permissions;

namespace Propely.OrgsApi.Api.Authorization;

/// <summary>
/// Authorization requirement that demands a specific permission.
/// </summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public Permission Permission { get; }

    public PermissionRequirement(Permission permission)
    {
        Permission = permission;
    }
}
