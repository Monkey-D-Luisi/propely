// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.OrgsApi.Application.Billing.Queries.GetPlans;

public sealed record GetPlansQuery() : IRequest<IReadOnlyList<PlanDto>>;

public sealed record PlanDto(string Id, string Name, List<string> Features);
