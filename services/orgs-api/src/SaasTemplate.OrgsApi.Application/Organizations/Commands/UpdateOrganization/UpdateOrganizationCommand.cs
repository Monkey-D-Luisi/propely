// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace SaasTemplate.OrgsApi.Application.Organizations.Commands.UpdateOrganization;

public sealed record UpdateOrganizationCommand(
    Guid OrgId,
    Guid RequestingUserId,
    string Name,
    string? Description) : IRequest<UpdateOrganizationResult>;

public sealed record UpdateOrganizationResult(Guid Id, string Name, string? Description);
