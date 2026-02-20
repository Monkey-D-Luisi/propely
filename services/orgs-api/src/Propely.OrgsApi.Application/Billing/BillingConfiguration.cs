// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Application.Billing;

public sealed class BillingConfiguration
{
    public const string SectionName = "Billing";

    public string Mode { get; set; } = "free";

    public StripeSettings Stripe { get; set; } = new();

    public List<PlanConfiguration> Plans { get; set; } = [];
}

public sealed class StripeSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}

public sealed class PlanConfiguration
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string StripePriceId { get; set; } = string.Empty;
    public int MaxMembers { get; set; }
    public int MaxOrganizations { get; set; }
    public List<string> Features { get; set; } = [];
}
