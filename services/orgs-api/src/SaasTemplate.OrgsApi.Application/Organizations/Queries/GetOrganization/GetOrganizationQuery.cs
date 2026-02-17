// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using SaasTemplate.OrgsApi.Application.Organizations.Queries.GetMyOrgs;

namespace SaasTemplate.OrgsApi.Application.Organizations.Queries.GetOrganization;

public sealed record GetOrganizationQuery(Guid OrgId, Guid RequestingUserId) : IRequest<OrgWithRole>;
