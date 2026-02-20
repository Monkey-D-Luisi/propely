// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.OrgsApi.Application.Billing.Queries.GetSubscription;

public sealed record GetSubscriptionQuery(Guid OrgId, Guid UserId) : IRequest<SubscriptionDto>;

public sealed record SubscriptionDto(
    string PlanId,
    string PlanName,
    string Status,
    DateTime? CurrentPeriodEnd,
    List<string> Features);
