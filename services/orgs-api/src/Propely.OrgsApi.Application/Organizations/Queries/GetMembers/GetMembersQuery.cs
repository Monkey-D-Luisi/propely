// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.OrgsApi.Application.Common.Models;

namespace Propely.OrgsApi.Application.Organizations.Queries.GetMembers;

public sealed record GetMembersQuery(Guid OrgId, Guid RequestingUserId, int Page = 1, int PageSize = 20, string? Search = null) : IRequest<PagedResult<MemberDto>>;

public sealed record MemberDto(Guid UserId, string Email, string? Name, string Role);
