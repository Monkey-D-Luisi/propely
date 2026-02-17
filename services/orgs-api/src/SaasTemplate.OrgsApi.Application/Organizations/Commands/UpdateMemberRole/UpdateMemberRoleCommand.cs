// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Domain.Organizations;
using MediatR;

namespace SaasTemplate.OrgsApi.Application.Organizations.Commands.UpdateMemberRole;

public sealed record UpdateMemberRoleCommand(Guid OrgId, Guid TargetUserId, Guid RequestingUserId, MembershipRole NewRole) : IRequest;
