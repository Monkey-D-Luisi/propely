// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Billing.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Billing;
using Propely.OrgsApi.Domain.Common.Exceptions;
using MediatR;

namespace Propely.OrgsApi.Application.Billing.Queries.GetSubscription;

public sealed class GetSubscriptionQueryHandler : IRequestHandler<GetSubscriptionQuery, SubscriptionDto>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IPlanProvider _planProvider;

    public GetSubscriptionQueryHandler(
        ISubscriptionRepository subscriptionRepository,
        IMembershipRepository membershipRepository,
        IPlanProvider planProvider)
    {
        _subscriptionRepository = subscriptionRepository;
        _membershipRepository = membershipRepository;
        _planProvider = planProvider;
    }

    public async Task<SubscriptionDto> Handle(GetSubscriptionQuery request, CancellationToken cancellationToken)
    {
        var membership = await _membershipRepository.GetAsync(request.OrgId, request.UserId, cancellationToken);

        if (membership is null)
            throw new ForbiddenException("User is not a member of this organization.");

        var subscription = await _subscriptionRepository.GetByOrgIdAsync(request.OrgId, cancellationToken);

        if (subscription is not null && subscription.Status is SubscriptionStatus.Active or SubscriptionStatus.Trialing)
        {
            var plan = _planProvider.GetPlan(subscription.PlanId);
            if (plan is not null)
            {
                return new SubscriptionDto(
                    plan.Id,
                    plan.Name,
                    subscription.Status.ToString().ToLowerInvariant(),
                    subscription.CurrentPeriodEnd,
                    plan.Features);
            }
        }

        var freePlan = _planProvider.GetFreePlan();
        return new SubscriptionDto(
            freePlan.Id,
            freePlan.Name,
            "free",
            null,
            freePlan.Features);
    }
}
