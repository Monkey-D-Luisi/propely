// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Domain.Common;

namespace SaasTemplate.OrgsApi.Domain.Billing;

public sealed class Payment : Entity
{
    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public string StripePaymentIntentId { get; private set; } = null!;
    public long Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public PaymentStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Payment() { }

    public static Payment Create(
        Guid organizationId,
        string stripePaymentIntentId,
        long amount,
        string currency,
        string description,
        PaymentStatus status)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            StripePaymentIntentId = stripePaymentIntentId.Trim(),
            Amount = amount,
            Currency = currency.Trim().ToLowerInvariant(),
            Description = description.Trim(),
            Status = status,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void MarkSucceeded()
    {
        Status = PaymentStatus.Succeeded;
    }

    public void MarkFailed()
    {
        Status = PaymentStatus.Failed;
    }
}
