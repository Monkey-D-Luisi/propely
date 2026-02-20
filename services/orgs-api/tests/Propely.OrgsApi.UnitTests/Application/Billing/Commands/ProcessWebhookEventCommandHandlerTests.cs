// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Billing.Commands.ProcessWebhookEvent;
using Propely.OrgsApi.Application.Billing.Interfaces;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Application.Users.Interfaces;
using Propely.OrgsApi.Domain.Billing;
using Propely.OrgsApi.Domain.Organizations;
using Propely.OrgsApi.Domain.Users;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Billing.Commands;

public sealed class ProcessWebhookEventCommandHandlerTests
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IWebhookEventRepository _webhookEventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly ILogger<ProcessWebhookEventCommandHandler> _logger;
    private readonly ProcessWebhookEventCommandHandler _handler;

    public ProcessWebhookEventCommandHandlerTests()
    {
        _subscriptionRepository = Substitute.For<ISubscriptionRepository>();
        _paymentRepository = Substitute.For<IPaymentRepository>();
        _webhookEventRepository = Substitute.For<IWebhookEventRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _membershipRepository = Substitute.For<IMembershipRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _emailService = Substitute.For<IEmailService>();
        _organizationRepository = Substitute.For<IOrganizationRepository>();
        _logger = Substitute.For<ILogger<ProcessWebhookEventCommandHandler>>();

        _handler = new ProcessWebhookEventCommandHandler(
            _subscriptionRepository,
            _paymentRepository,
            _webhookEventRepository,
            _unitOfWork,
            _membershipRepository,
            _userRepository,
            _emailService,
            _organizationRepository,
            _logger);
    }

    [Fact]
    public async Task Handle_AlreadyProcessedEvent_ShouldSkip()
    {
        // Arrange
        var command = CreateCommand("evt_123", "checkout.session.completed");
        _webhookEventRepository.IsProcessedAsync("evt_123", Arg.Any<CancellationToken>()).Returns(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CheckoutSessionCompleted_NewSubscription_ShouldCreate()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var periodEnd = DateTime.UtcNow.AddMonths(1);
        var data = new WebhookEventData
        {
            OrgId = orgId,
            StripeCustomerId = "cus_abc",
            StripeSubscriptionId = "sub_xyz",
            PlanId = "pro",
            CurrentPeriodEnd = periodEnd
        };

        var command = CreateCommand("evt_1", "checkout.session.completed", data);
        _webhookEventRepository.IsProcessedAsync("evt_1", Arg.Any<CancellationToken>()).Returns(false);
        _subscriptionRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>()).Returns((Subscription?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _subscriptionRepository.Received(1).AddAsync(
            Arg.Is<Subscription>(s =>
                s.OrganizationId == orgId &&
                s.StripeCustomerId == "cus_abc" &&
                s.StripeSubscriptionId == "sub_xyz" &&
                s.PlanId == "pro" &&
                s.Status == SubscriptionStatus.Active),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CheckoutSessionCompleted_ExistingSubscription_ShouldUpdate()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var existing = Subscription.Create(orgId, "cus_old", "sub_old", "starter", SubscriptionStatus.PastDue, DateTime.UtcNow);
        var data = new WebhookEventData
        {
            OrgId = orgId,
            StripeCustomerId = "cus_abc",
            StripeSubscriptionId = "sub_xyz",
            PlanId = "pro",
            CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1)
        };

        var command = CreateCommand("evt_2", "checkout.session.completed", data);
        _webhookEventRepository.IsProcessedAsync("evt_2", Arg.Any<CancellationToken>()).Returns(false);
        _subscriptionRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(existing);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existing.Status.Should().Be(SubscriptionStatus.Active);
        existing.PlanId.Should().Be("pro");
        await _subscriptionRepository.DidNotReceive().AddAsync(Arg.Any<Subscription>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CheckoutSessionCompleted_MissingRequiredFields_ShouldSkipProcessing()
    {
        // Arrange
        var data = new WebhookEventData { OrgId = null, StripeCustomerId = null, StripeSubscriptionId = null };
        var command = CreateCommand("evt_3", "checkout.session.completed", data);
        _webhookEventRepository.IsProcessedAsync("evt_3", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _subscriptionRepository.DidNotReceive().GetByOrgIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _webhookEventRepository.Received(1).MarkProcessedAsync("evt_3", "checkout.session.completed", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InvoicePaid_ShouldUpdateToActive()
    {
        // Arrange
        var periodEnd = DateTime.UtcNow.AddMonths(1);
        var sub = Subscription.Create(Guid.NewGuid(), "cus_1", "sub_inv", "pro", SubscriptionStatus.PastDue, DateTime.UtcNow);
        var data = new WebhookEventData
        {
            StripeSubscriptionId = "sub_inv",
            CurrentPeriodEnd = periodEnd
        };

        var command = CreateCommand("evt_4", "invoice.paid", data);
        _webhookEventRepository.IsProcessedAsync("evt_4", Arg.Any<CancellationToken>()).Returns(false);
        _subscriptionRepository.GetByStripeSubscriptionIdAsync("sub_inv", Arg.Any<CancellationToken>()).Returns(sub);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        sub.Status.Should().Be(SubscriptionStatus.Active);
        sub.CurrentPeriodEnd.Should().Be(periodEnd);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InvoicePaid_SubscriptionNotFound_ShouldReturn()
    {
        // Arrange
        var data = new WebhookEventData { StripeSubscriptionId = "sub_unknown" };
        var command = CreateCommand("evt_5", "invoice.paid", data);
        _webhookEventRepository.IsProcessedAsync("evt_5", Arg.Any<CancellationToken>()).Returns(false);
        _subscriptionRepository.GetByStripeSubscriptionIdAsync("sub_unknown", Arg.Any<CancellationToken>())
            .Returns((Subscription?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - should still mark processed and save
        await _webhookEventRepository.Received(1).MarkProcessedAsync("evt_5", "invoice.paid", Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InvoicePaymentFailed_ShouldUpdateToPastDue()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var sub = Subscription.Create(orgId, "cus_1", "sub_fail", "pro", SubscriptionStatus.Active, DateTime.UtcNow.AddMonths(1));
        var data = new WebhookEventData { StripeSubscriptionId = "sub_fail" };

        var command = CreateCommand("evt_6", "invoice.payment_failed", data);
        _webhookEventRepository.IsProcessedAsync("evt_6", Arg.Any<CancellationToken>()).Returns(false);
        _subscriptionRepository.GetByStripeSubscriptionIdAsync("sub_fail", Arg.Any<CancellationToken>()).Returns(sub);
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(Organization.Create("Test Org"));
        _membershipRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(new List<Membership>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        sub.Status.Should().Be(SubscriptionStatus.PastDue);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InvoicePaymentFailed_ShouldSendEmailToOwners()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var sub = Subscription.Create(orgId, "cus_1", "sub_fail2", "pro", SubscriptionStatus.Active, DateTime.UtcNow.AddMonths(1));
        var data = new WebhookEventData { StripeSubscriptionId = "sub_fail2" };

        var command = CreateCommand("evt_7", "invoice.payment_failed", data);
        _webhookEventRepository.IsProcessedAsync("evt_7", Arg.Any<CancellationToken>()).Returns(false);
        _subscriptionRepository.GetByStripeSubscriptionIdAsync("sub_fail2", Arg.Any<CancellationToken>()).Returns(sub);

        var org = Organization.Create("Acme Corp");
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(org);

        var ownerMembership = Membership.Create(ownerId, orgId, MembershipRole.Owner);
        var memberMembership = Membership.Create(Guid.NewGuid(), orgId, MembershipRole.Member);
        _membershipRepository.GetByOrgIdAsync(orgId, Arg.Any<CancellationToken>())
            .Returns(new List<Membership> { ownerMembership, memberMembership });

        var owner = User.Create("owner@acme.com", "hash123");
        _userRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<User> { owner });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _emailService.Received(1).SendPaymentFailureEmailAsync(
            "owner@acme.com",
            "Acme Corp",
            Arg.Any<CancellationToken>(),
            Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_SubscriptionUpdated_ShouldUpdatePlanAndStatus()
    {
        // Arrange
        var sub = Subscription.Create(Guid.NewGuid(), "cus_1", "sub_upd", "starter", SubscriptionStatus.Active, DateTime.UtcNow);
        var periodEnd = DateTime.UtcNow.AddMonths(1);
        var data = new WebhookEventData
        {
            StripeSubscriptionId = "sub_upd",
            StripeStatus = "active",
            PlanId = "pro",
            CurrentPeriodEnd = periodEnd
        };

        var command = CreateCommand("evt_8", "customer.subscription.updated", data);
        _webhookEventRepository.IsProcessedAsync("evt_8", Arg.Any<CancellationToken>()).Returns(false);
        _subscriptionRepository.GetByStripeSubscriptionIdAsync("sub_upd", Arg.Any<CancellationToken>()).Returns(sub);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        sub.PlanId.Should().Be("pro");
        sub.Status.Should().Be(SubscriptionStatus.Active);
        sub.CurrentPeriodEnd.Should().Be(periodEnd);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SubscriptionUpdated_PastDueStatus_ShouldMapCorrectly()
    {
        // Arrange
        var sub = Subscription.Create(Guid.NewGuid(), "cus_1", "sub_pd", "pro", SubscriptionStatus.Active, DateTime.UtcNow);
        var data = new WebhookEventData
        {
            StripeSubscriptionId = "sub_pd",
            StripeStatus = "past_due"
        };

        var command = CreateCommand("evt_9", "customer.subscription.updated", data);
        _webhookEventRepository.IsProcessedAsync("evt_9", Arg.Any<CancellationToken>()).Returns(false);
        _subscriptionRepository.GetByStripeSubscriptionIdAsync("sub_pd", Arg.Any<CancellationToken>()).Returns(sub);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        sub.Status.Should().Be(SubscriptionStatus.PastDue);
    }

    [Fact]
    public async Task Handle_SubscriptionDeleted_ShouldCancel()
    {
        // Arrange
        var sub = Subscription.Create(Guid.NewGuid(), "cus_1", "sub_del", "pro", SubscriptionStatus.Active, DateTime.UtcNow.AddMonths(1));
        var data = new WebhookEventData { StripeSubscriptionId = "sub_del" };

        var command = CreateCommand("evt_10", "customer.subscription.deleted", data);
        _webhookEventRepository.IsProcessedAsync("evt_10", Arg.Any<CancellationToken>()).Returns(false);
        _subscriptionRepository.GetByStripeSubscriptionIdAsync("sub_del", Arg.Any<CancellationToken>()).Returns(sub);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        sub.Status.Should().Be(SubscriptionStatus.Cancelled);
        sub.CancelledAtUtc.Should().NotBeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownEventType_ShouldBeIgnored()
    {
        // Arrange
        var command = CreateCommand("evt_11", "unknown.event.type");
        _webhookEventRepository.IsProcessedAsync("evt_11", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _webhookEventRepository.DidNotReceive()
            .MarkProcessedAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PaymentCheckoutCompleted_ShouldCreatePayment()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var data = new WebhookEventData
        {
            OrgId = orgId,
            CheckoutMode = "payment",
            StripePaymentIntentId = "pi_abc123",
            Amount = 5000,
            Currency = "usd"
        };

        var command = CreateCommand("evt_pay_1", "checkout.session.completed", data);
        _webhookEventRepository.IsProcessedAsync("evt_pay_1", Arg.Any<CancellationToken>()).Returns(false);
        _paymentRepository.GetByStripePaymentIntentIdAsync("pi_abc123", Arg.Any<CancellationToken>())
            .Returns((Payment?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _paymentRepository.Received(1).AddAsync(
            Arg.Is<Payment>(p =>
                p.OrganizationId == orgId &&
                p.StripePaymentIntentId == "pi_abc123" &&
                p.Amount == 5000 &&
                p.Currency == "usd" &&
                p.Status == PaymentStatus.Succeeded),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PaymentCheckoutCompleted_DuplicatePaymentIntent_ShouldSkip()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var existing = Payment.Create(orgId, "pi_dup", 5000, "usd", "Already processed", PaymentStatus.Succeeded);
        var data = new WebhookEventData
        {
            OrgId = orgId,
            CheckoutMode = "payment",
            StripePaymentIntentId = "pi_dup",
            Amount = 5000,
            Currency = "usd"
        };

        var command = CreateCommand("evt_pay_2", "checkout.session.completed", data);
        _webhookEventRepository.IsProcessedAsync("evt_pay_2", Arg.Any<CancellationToken>()).Returns(false);
        _paymentRepository.GetByStripePaymentIntentIdAsync("pi_dup", Arg.Any<CancellationToken>()).Returns(existing);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _paymentRepository.DidNotReceive().AddAsync(Arg.Any<Payment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PaymentCheckoutCompleted_MissingFields_ShouldSkip()
    {
        // Arrange
        var data = new WebhookEventData
        {
            OrgId = null,
            CheckoutMode = "payment",
            StripePaymentIntentId = null
        };

        var command = CreateCommand("evt_pay_3", "checkout.session.completed", data);
        _webhookEventRepository.IsProcessedAsync("evt_pay_3", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _paymentRepository.DidNotReceive().AddAsync(Arg.Any<Payment>(), Arg.Any<CancellationToken>());
    }

    private static ProcessWebhookEventCommand CreateCommand(
        string eventId,
        string eventType,
        WebhookEventData? data = null)
    {
        return new ProcessWebhookEventCommand(eventId, eventType, data ?? new WebhookEventData());
    }
}
