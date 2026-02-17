// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace SaasTemplate.OrgsApi.Application.Billing.Interfaces;

public interface IPlanProvider
{
    PlanInfo? GetPlan(string planId);
    PlanInfo GetFreePlan();
    IReadOnlyList<PlanInfo> GetAllPlans();
    bool IsBillingEnabled { get; }
}

public sealed record PlanInfo(
    string Id,
    string Name,
    int MaxMembers,
    int MaxOrganizations,
    List<string> Features);
