// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Billing;
using Propely.OrgsApi.Application.Billing.Interfaces;
using Microsoft.Extensions.Options;

namespace Propely.OrgsApi.Infrastructure.Billing;

public sealed class PlanProvider : IPlanProvider
{
    private readonly BillingConfiguration _config;

    public PlanProvider(IOptions<BillingConfiguration> config)
    {
        _config = config.Value;
    }

    public bool IsBillingEnabled =>
        string.Equals(_config.Mode, "stripe", StringComparison.OrdinalIgnoreCase);

    public PlanInfo? GetPlan(string planId)
    {
        var plan = _config.Plans.Find(p =>
            string.Equals(p.Id, planId, StringComparison.OrdinalIgnoreCase));

        return plan is null ? null : ToPlanInfo(plan);
    }

    public PlanInfo GetFreePlan()
    {
        var plan = _config.Plans.Find(p =>
            string.Equals(p.Id, "free", StringComparison.OrdinalIgnoreCase));

        return plan is not null
            ? ToPlanInfo(plan)
            : new PlanInfo("free", "Free", 5, 1, ["5 members", "1 org"]);
    }

    public IReadOnlyList<PlanInfo> GetAllPlans()
    {
        return _config.Plans.Select(ToPlanInfo).ToList();
    }

    private static PlanInfo ToPlanInfo(PlanConfiguration plan) =>
        new(plan.Id, plan.Name, plan.MaxMembers, plan.MaxOrganizations, new List<string>(plan.Features));
}
