// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Common;

namespace Propely.OrgsApi.Domain.Billing;

public sealed class Subscription : Entity
{
    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public string StripeCustomerId { get; private set; } = null!;
    public string StripeSubscriptionId { get; private set; } = null!;
    public string PlanId { get; private set; } = null!;
    public SubscriptionStatus Status { get; private set; }
    public DateTime CurrentPeriodEnd { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private Subscription() { }

    public static Subscription Create(
        Guid organizationId,
        string stripeCustomerId,
        string stripeSubscriptionId,
        string planId,
        SubscriptionStatus status,
        DateTime currentPeriodEnd)
    {
        return new Subscription
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            StripeCustomerId = stripeCustomerId.Trim(),
            StripeSubscriptionId = stripeSubscriptionId.Trim(),
            PlanId = planId.Trim(),
            Status = status,
            CurrentPeriodEnd = currentPeriodEnd,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void UpdateStatus(SubscriptionStatus status, DateTime currentPeriodEnd)
    {
        Status = status;
        CurrentPeriodEnd = currentPeriodEnd;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdatePlan(string planId)
    {
        PlanId = planId.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Cancel()
    {
        var now = DateTime.UtcNow;
        Status = SubscriptionStatus.Cancelled;
        CancelledAtUtc = now;
        UpdatedAtUtc = now;
    }
}
