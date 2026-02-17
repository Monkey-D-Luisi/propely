// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace SaasTemplate.OrgsApi.Application.Organizations.Commands.LeaveOrganization;

public sealed record LeaveOrganizationCommand(Guid OrgId, Guid UserId) : IRequest;
