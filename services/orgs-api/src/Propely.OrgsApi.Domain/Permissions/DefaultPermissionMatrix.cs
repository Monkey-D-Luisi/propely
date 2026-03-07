// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Organizations;

namespace Propely.OrgsApi.Domain.Permissions;

/// <summary>
/// Maps each MembershipRole to its default set of granted permissions.
/// </summary>
public static class DefaultPermissionMatrix
{
    private static readonly IReadOnlySet<Permission> OwnerDefaults =
        new HashSet<Permission>(Enum.GetValues<Permission>()).AsReadOnly();

    // INTENTIONAL: Admin and Owner have identical default permissions today.
    // They will be differentiated when billing/subscription features arrive —
    // Owner will gain exclusive access to billing management, plan changes,
    // and org deletion. Until then, both roles grant all permissions.
    private static readonly IReadOnlySet<Permission> AdminDefaults =
        new HashSet<Permission>(Enum.GetValues<Permission>()).AsReadOnly();

    private static readonly IReadOnlySet<Permission> AgentDefaults =
        new HashSet<Permission> { Permission.LeadsManage }.AsReadOnly();

    private static readonly IReadOnlySet<Permission> ViewerDefaults =
        new HashSet<Permission>().AsReadOnly();

    /// <summary>
    /// Returns the default permissions for the given role.
    /// </summary>
    public static IReadOnlySet<Permission> GetDefaults(MembershipRole role) => role switch
    {
        MembershipRole.Owner => OwnerDefaults,
        MembershipRole.Admin => AdminDefaults,
        MembershipRole.Agent => AgentDefaults,
        MembershipRole.Viewer => ViewerDefaults,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown membership role.")
    };
}
