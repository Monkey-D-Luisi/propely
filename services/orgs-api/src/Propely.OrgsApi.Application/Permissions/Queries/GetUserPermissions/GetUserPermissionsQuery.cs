// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.OrgsApi.Application.Permissions.DTOs;

namespace Propely.OrgsApi.Application.Permissions.Queries.GetUserPermissions;

/// <summary>
/// Query to retrieve effective permissions for a user in an organization.
/// </summary>
public sealed record GetUserPermissionsQuery(
    Guid OrganizationId,
    Guid TargetUserId,
    Guid RequestingUserId) : IRequest<IReadOnlyList<EffectivePermissionDto>>;
