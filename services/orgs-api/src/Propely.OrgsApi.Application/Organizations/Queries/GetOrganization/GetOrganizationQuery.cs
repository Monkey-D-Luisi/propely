// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.OrgsApi.Application.Organizations.Queries.GetMyOrgs;

namespace Propely.OrgsApi.Application.Organizations.Queries.GetOrganization;

public sealed record GetOrganizationQuery(Guid OrgId, Guid RequestingUserId) : IRequest<OrgWithRole>;
