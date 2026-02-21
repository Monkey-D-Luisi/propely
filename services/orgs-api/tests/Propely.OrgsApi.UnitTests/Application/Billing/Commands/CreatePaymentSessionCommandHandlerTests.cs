// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Billing.Commands.CreatePaymentSession;
using Propely.OrgsApi.Application.Billing.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Organizations;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Billing.Commands;

public sealed class CreatePaymentSessionCommandHandlerTests
{
    private readonly IPaymentService _paymentService;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly CreatePaymentSessionCommandHandler _handler;

    public CreatePaymentSessionCommandHandlerTests()
    {
        _paymentService = Substitute.For<IPaymentService>();
        _organizationRepository = Substitute.For<IOrganizationRepository>();
        _membershipRepository = Substitute.For<IMembershipRepository>();
        _handler = new CreatePaymentSessionCommandHandler(
            _paymentService, _organizationRepository, _membershipRepository);
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
        _paymentService.CreatePaymentCheckoutSessionAsync(
            orgId, 5000, "usd", "Add-on feature",
            "https://example.com/success", "https://example.com/cancel",
            Arg.Any<CancellationToken>())
            .Returns("https://checkout.stripe.com/pay_session123");

        var command = new CreatePaymentSessionCommand(
            orgId, userId, 5000, "usd", "Add-on feature",
            "https://example.com/success", "https://example.com/cancel");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CheckoutUrl.Should().Be("https://checkout.stripe.com/pay_session123");
    }

    [Fact]
    public async Task Handle_WhenOrgNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>()).Returns((Organization?)null);

        var command = new CreatePaymentSessionCommand(
            orgId, userId, 5000, "usd", "Test", "https://example.com/success", "https://example.com/cancel");

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

        var command = new CreatePaymentSessionCommand(
            orgId, userId, 5000, "usd", "Test", "https://example.com/success", "https://example.com/cancel");

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
        var membership = Membership.Create(userId, orgId, MembershipRole.Agent);

        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(org);
        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>()).Returns(membership);

        var command = new CreatePaymentSessionCommand(
            orgId, userId, 5000, "usd", "Test", "https://example.com/success", "https://example.com/cancel");

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

        var command = new CreatePaymentSessionCommand(
            orgId, userId, 5000, "usd", "Test", "https://example.com/success", "https://example.com/cancel");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
    }
}
