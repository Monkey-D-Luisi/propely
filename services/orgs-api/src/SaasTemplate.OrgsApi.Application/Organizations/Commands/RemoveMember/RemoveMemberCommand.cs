// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace SaasTemplate.OrgsApi.Application.Organizations.Commands.RemoveMember;

public sealed record RemoveMemberCommand(Guid OrgId, Guid TargetUserId, Guid RequestingUserId) : IRequest;
