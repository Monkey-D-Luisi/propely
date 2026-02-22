// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.AspNetCore.Authorization;
using Propely.OrgsApi.Domain.Permissions;

namespace Propely.OrgsApi.Api.Authorization;

/// <summary>
/// Attribute that enforces a specific permission on a controller action.
/// Usage: [RequirePermission(Permission.PropertiesViewAll)]
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission:";

    public RequirePermissionAttribute(Permission permission)
        : base($"{PolicyPrefix}{permission}")
    {
    }
}
