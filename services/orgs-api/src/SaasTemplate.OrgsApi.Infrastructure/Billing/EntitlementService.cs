// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Billing.Interfaces;
using SaasTemplate.OrgsApi.Application.Organizations.Interfaces;
using SaasTemplate.OrgsApi.Domain.Billing;

namespace SaasTemplate.OrgsApi.Infrastructure.Billing;

public sealed class EntitlementService : IEntitlementService
{
    private readonly IPlanProvider _planProvider;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMembershipRepository _membershipRepository;

    public EntitlementService(
        IPlanProvider planProvider,
        ISubscriptionRepository subscriptionRepository,
        IMembershipRepository membershipRepository)
    {
        _planProvider = planProvider;
        _subscriptionRepository = subscriptionRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<bool> CanCreateOrganizationAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (!_planProvider.IsBillingEnabled)
            return true;

        var memberships = await _membershipRepository.GetByUserIdAsync(userId, cancellationToken);
        var maxOrgs = _planProvider.GetFreePlan().MaxOrganizations;

        foreach (var membership in memberships)
        {
            var subscription = await _subscriptionRepository.GetByOrgIdAsync(
                membership.OrganizationId, cancellationToken);

            if (subscription is not null &&
                subscription.Status is SubscriptionStatus.Active or SubscriptionStatus.Trialing)
            {
                var plan = _planProvider.GetPlan(subscription.PlanId);
                if (plan is not null)
                {
                    if (plan.MaxOrganizations == 0)
                        return true;

                    if (plan.MaxOrganizations > maxOrgs)
                        maxOrgs = plan.MaxOrganizations;
                }
            }
        }

        if (maxOrgs == 0)
            return true;

        return memberships.Count < maxOrgs;
    }

    public async Task<bool> CanAddMemberAsync(Guid orgId, CancellationToken cancellationToken = default)
    {
        if (!_planProvider.IsBillingEnabled)
            return true;

        var plan = await GetEffectivePlanAsync(orgId, cancellationToken);

        if (plan.MaxMembers == 0)
            return true;

        var members = await _membershipRepository.GetByOrgIdAsync(orgId, cancellationToken);
        return members.Count < plan.MaxMembers;
    }

    private async Task<PlanInfo> GetEffectivePlanAsync(Guid orgId, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByOrgIdAsync(orgId, cancellationToken);

        if (subscription is not null &&
            subscription.Status is SubscriptionStatus.Active or SubscriptionStatus.Trialing)
        {
            var plan = _planProvider.GetPlan(subscription.PlanId);
            if (plan is not null)
                return plan;
        }

        return _planProvider.GetFreePlan();
    }
}
