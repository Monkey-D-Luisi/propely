// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Billing.Commands.CreateCheckoutSession;
using Propely.OrgsApi.Application.Billing.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Organizations;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Billing.Commands;

public sealed class CreateCheckoutSessionCommandHandlerTests
{
    private readonly IPaymentService _paymentService;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IPlanProvider _planProvider;
    private readonly CreateCheckoutSessionCommandHandler _handler;

    public CreateCheckoutSessionCommandHandlerTests()
    {
        _paymentService = Substitute.For<IPaymentService>();
        _organizationRepository = Substitute.For<IOrganizationRepository>();
        _membershipRepository = Substitute.For<IMembershipRepository>();
        _planProvider = Substitute.For<IPlanProvider>();
        _planProvider.GetPlan("pro").Returns(new PlanInfo("pro", "Pro", 0, 5, ["Unlimited members", "5 orgs"]));
        _handler = new CreateCheckoutSessionCommandHandler(
            _paymentService, _organizationRepository, _membershipRepository, _planProvider);
    }

    [Fact]
    public async Task Handle_WithValidOwner_ShouldReturnCheckoutUrl()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var org = Organization.Create("Test Org");
        var membership = Membership.Create(userId, orgId, MembershipRole.Owner);

        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(org);
        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>()).Returns(membership);
        _paymentService.IsEnabled.Returns(true);
        _paymentService.CreateCheckoutSessionAsync(orgId, "pro", "https://example.com/success", "https://example.com/cancel", Arg.Any<CancellationToken>())
            .Returns("https://checkout.stripe.com/session123");

        var command = new CreateCheckoutSessionCommand(orgId, userId, "pro", "https://example.com/success", "https://example.com/cancel");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CheckoutUrl.Should().Be("https://checkout.stripe.com/session123");
    }

    [Fact]
    public async Task Handle_WithAdmin_ShouldReturnCheckoutUrl()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var org = Organization.Create("Test Org");
        var membership = Membership.Create(userId, orgId, MembershipRole.Admin);

        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(org);
        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>()).Returns(membership);
        _paymentService.IsEnabled.Returns(true);
        _paymentService.CreateCheckoutSessionAsync(orgId, "pro", "https://example.com/success", "https://example.com/cancel", Arg.Any<CancellationToken>())
            .Returns("https://checkout.stripe.com/session456");

        var command = new CreateCheckoutSessionCommand(orgId, userId, "pro", "https://example.com/success", "https://example.com/cancel");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CheckoutUrl.Should().Be("https://checkout.stripe.com/session456");
    }

    [Fact]
    public async Task Handle_WhenOrgNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>()).Returns((Organization?)null);

        var command = new CreateCheckoutSessionCommand(orgId, userId, "pro", "https://example.com/success", "https://example.com/cancel");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserNotMember_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var org = Organization.Create("Test Org");

        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(org);
        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>()).Returns((Membership?)null);

        var command = new CreateCheckoutSessionCommand(orgId, userId, "pro", "https://example.com/success", "https://example.com/cancel");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("User is not a member of this organization.");
    }

    [Fact]
    public async Task Handle_WhenUserIsMember_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var org = Organization.Create("Test Org");
        var membership = Membership.Create(userId, orgId, MembershipRole.Member);

        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(org);
        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>()).Returns(membership);

        var command = new CreateCheckoutSessionCommand(orgId, userId, "pro", "https://example.com/success", "https://example.com/cancel");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Only owners and admins can manage billing.");
    }

    [Fact]
    public async Task Handle_WhenBillingDisabled_ShouldThrowDomainException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var org = Organization.Create("Test Org");
        var membership = Membership.Create(userId, orgId, MembershipRole.Owner);

        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(org);
        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>()).Returns(membership);
        _paymentService.IsEnabled.Returns(false);

        var command = new CreateCheckoutSessionCommand(orgId, userId, "pro", "https://example.com/success", "https://example.com/cancel");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_WhenPlanIdUnknown_ShouldThrowDomainException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var org = Organization.Create("Test Org");
        var membership = Membership.Create(userId, orgId, MembershipRole.Owner);

        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(org);
        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>()).Returns(membership);
        _paymentService.IsEnabled.Returns(true);
        _planProvider.GetPlan("nonexistent").Returns((PlanInfo?)null);

        var command = new CreateCheckoutSessionCommand(orgId, userId, "nonexistent", "https://example.com/success", "https://example.com/cancel");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Unknown plan 'nonexistent'.");
    }
}
