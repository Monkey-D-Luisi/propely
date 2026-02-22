// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.OrgsApi.Domain.Permissions;

namespace Propely.OrgsApi.Application.Permissions.Commands.RemovePermissionOverride;

/// <summary>
/// Command to remove a permission override, reverting to role default.
/// </summary>
public sealed record RemovePermissionOverrideCommand(
    Guid OrganizationId,
    Guid TargetUserId,
    Permission Permission,
    Guid RequestingUserId) : IRequest;
