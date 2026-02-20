// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Infrastructure.Billing;
using FluentAssertions;
using Stripe;
using Stripe.Checkout;

namespace Propely.OrgsApi.UnitTests.Infrastructure.Billing;

public sealed class WebhookEventMapperTests
{
    private static readonly Dictionary<string, string> PriceIdToPlanId = new()
    {
        ["price_pro"] = "pro",
        ["price_enterprise"] = "enterprise"
    };

    [Fact]
    public void MapToEventData_CheckoutSessionCompleted_ShouldMapFields()
    {
        var orgId = Guid.NewGuid();
        var session = new Session
        {
            CustomerId = "cus_123",
            SubscriptionId = "sub_456",
            Metadata = new Dictionary<string, string>
            {
                ["org_id"] = orgId.ToString(),
                ["plan_id"] = "pro"
            }
        };

        var stripeEvent = new Event
        {
            Type = "checkout.session.completed",
            Data = new EventData { Object = session }
        };

        var result = WebhookEventMapper.MapToEventDataStatic(stripeEvent, PriceIdToPlanId);

        result.OrgId.Should().Be(orgId);
        result.StripeCustomerId.Should().Be("cus_123");
        result.StripeSubscriptionId.Should().Be("sub_456");
        result.PlanId.Should().Be("pro");
    }

    [Fact]
    public void MapToEventData_CheckoutSessionCompleted_WithMissingMetadata_ShouldReturnNulls()
    {
        var session = new Session
        {
            CustomerId = "cus_123",
            Metadata = new Dictionary<string, string>()
        };

        var stripeEvent = new Event
        {
            Type = "checkout.session.completed",
            Data = new EventData { Object = session }
        };

        var result = WebhookEventMapper.MapToEventDataStatic(stripeEvent);

        result.OrgId.Should().BeNull();
        result.PlanId.Should().BeNull();
        result.StripeCustomerId.Should().Be("cus_123");
    }

    [Fact]
    public void MapToEventData_CheckoutSessionCompleted_WithInvalidOrgId_ShouldReturnNullOrgId()
    {
        var session = new Session
        {
            Metadata = new Dictionary<string, string>
            {
                ["org_id"] = "not-a-guid"
            }
        };

        var stripeEvent = new Event
        {
            Type = "checkout.session.completed",
            Data = new EventData { Object = session }
        };

        var result = WebhookEventMapper.MapToEventDataStatic(stripeEvent);

        result.OrgId.Should().BeNull();
    }

    [Fact]
    public void MapToEventData_SubscriptionUpdated_ShouldMapFieldsWithPriceMapping()
    {
        var subscription = new Stripe.Subscription
        {
            Id = "sub_789",
            Status = "active",
            Items = new StripeList<SubscriptionItem>
            {
                Data =
                [
                    new SubscriptionItem
                    {
                        Price = new Price { Id = "price_pro" },
                        CurrentPeriodEnd = new DateTime(2026, 3, 11, 0, 0, 0, DateTimeKind.Utc)
                    }
                ]
            }
        };

        var stripeEvent = new Event
        {
            Type = "customer.subscription.updated",
            Data = new EventData { Object = subscription }
        };

        var result = WebhookEventMapper.MapToEventDataStatic(stripeEvent, PriceIdToPlanId);

        result.StripeSubscriptionId.Should().Be("sub_789");
        result.StripeStatus.Should().Be("active");
        result.PlanId.Should().Be("pro");
        result.CurrentPeriodEnd.Should().NotBeNull();
    }

    [Fact]
    public void MapToEventData_SubscriptionUpdated_WithUnknownPriceId_ShouldReturnNullPlanId()
    {
        var subscription = new Stripe.Subscription
        {
            Id = "sub_789",
            Status = "active",
            Items = new StripeList<SubscriptionItem>
            {
                Data =
                [
                    new SubscriptionItem
                    {
                        Price = new Price { Id = "price_unknown" }
                    }
                ]
            }
        };

        var stripeEvent = new Event
        {
            Type = "customer.subscription.updated",
            Data = new EventData { Object = subscription }
        };

        var result = WebhookEventMapper.MapToEventDataStatic(stripeEvent, PriceIdToPlanId);

        result.PlanId.Should().BeNull();
    }

    [Fact]
    public void MapToEventData_SubscriptionDeleted_ShouldMapFields()
    {
        var subscription = new Stripe.Subscription
        {
            Id = "sub_del",
            Status = "canceled"
        };

        var stripeEvent = new Event
        {
            Type = "customer.subscription.deleted",
            Data = new EventData { Object = subscription }
        };

        var result = WebhookEventMapper.MapToEventDataStatic(stripeEvent);

        result.StripeSubscriptionId.Should().Be("sub_del");
        result.StripeStatus.Should().Be("canceled");
    }

    [Fact]
    public void MapToEventData_UnknownEventType_ShouldReturnEmptyData()
    {
        var stripeEvent = new Event
        {
            Type = "unknown.event.type",
            Data = new EventData()
        };

        var result = WebhookEventMapper.MapToEventDataStatic(stripeEvent);

        result.OrgId.Should().BeNull();
        result.StripeCustomerId.Should().BeNull();
        result.StripeSubscriptionId.Should().BeNull();
        result.PlanId.Should().BeNull();
    }

    [Fact]
    public void MapToEventData_CheckoutWithWrongObjectType_ShouldReturnEmptyData()
    {
        var stripeEvent = new Event
        {
            Type = "checkout.session.completed",
            Data = new EventData { Object = new Stripe.Subscription() }
        };

        var result = WebhookEventMapper.MapToEventDataStatic(stripeEvent);

        result.OrgId.Should().BeNull();
        result.StripeCustomerId.Should().BeNull();
    }
}
