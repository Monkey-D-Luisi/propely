// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Billing.Interfaces;
using MediatR;

namespace Propely.OrgsApi.Application.Billing.Queries.GetPlans;

public sealed class GetPlansQueryHandler : IRequestHandler<GetPlansQuery, IReadOnlyList<PlanDto>>
{
    private readonly IPlanProvider _planProvider;

    public GetPlansQueryHandler(IPlanProvider planProvider)
    {
        _planProvider = planProvider;
    }

    public Task<IReadOnlyList<PlanDto>> Handle(GetPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = _planProvider.GetAllPlans();

        IReadOnlyList<PlanDto> result = plans
            .Select(p => new PlanDto(p.Id, p.Name, new List<string>(p.Features)))
            .ToList();

        return Task.FromResult(result);
    }
}
