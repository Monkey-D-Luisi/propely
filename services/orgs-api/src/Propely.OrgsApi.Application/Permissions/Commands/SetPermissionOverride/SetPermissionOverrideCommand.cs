// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.OrgsApi.Domain.Permissions;

namespace Propely.OrgsApi.Application.Permissions.Commands.SetPermissionOverride;

/// <summary>
/// Command to set (create or update) a permission override for a user.
/// </summary>
public sealed record SetPermissionOverrideCommand(
    Guid OrganizationId,
    Guid TargetUserId,
    Permission Permission,
    bool Granted,
    Guid RequestingUserId) : IRequest;
