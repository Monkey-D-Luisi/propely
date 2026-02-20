// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.OrgsApi.Application.Common.Models;

namespace Propely.OrgsApi.Application.Organizations.Queries.GetMyOrgs;

public sealed record GetMyOrgsQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<PagedResult<OrgWithRole>>;

public sealed record OrgWithRole(Guid Id, string Name, string? Description, string? Role);
