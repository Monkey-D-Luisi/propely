// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Domain.Organizations;
using MediatR;

namespace SaasTemplate.OrgsApi.Application.Organizations.Commands.CreateInvitation;

public sealed record CreateInvitationCommand(Guid OrgId, string Email, Guid RequestingUserId, MembershipRole Role) : IRequest<CreateInvitationResult>;

public sealed record CreateInvitationResult(bool Ok, string? InviteUrl);
