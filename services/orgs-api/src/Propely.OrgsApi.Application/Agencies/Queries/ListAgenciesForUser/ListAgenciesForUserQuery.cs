// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Agencies.DTOs;
using MediatR;

namespace Propely.OrgsApi.Application.Agencies.Queries.ListAgenciesForUser;

public sealed record ListAgenciesForUserQuery(Guid UserId) : IRequest<List<AgencyDto>>;
