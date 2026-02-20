// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Billing.Commands.CreateCustomerPortalSession;
using Propely.OrgsApi.Application.Billing.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Organizations;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Billing.Commands;

public sealed class CreateCustomerPortalSessionCommandHandlerTests
{
    private readonly IPaymentService _paymentService;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly CreateCustomerPortalSessionCommandHandler _handler;

    public CreateCustomerPortalSessionCommandHandlerTests()
    {
        _paymentService = Substitute.For<IPaymentService>();
        _organizationRepository = Substitute.For<IOrganizationRepository>();
        _membershipRepository = Substitute.For<IMembershipRepository>();
        _handler = new CreateCustomerPortalSessionCommandHandler(
            _paymentService, _organizationRepository, _membershipRepository);
    }

    [Fact]
    public async Task Handle_WithValidOwner_ShouldReturnPortalUrl()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var org = Organization.Create("Test Org");
        var membership = Membership.Create(userId, orgId, MembershipRole.Owner);

        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(org);
        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>()).Returns(membership);
        _paymentService.IsEnabled.Returns(true);
        _paymentService.CreateCustomerPortalSessionAsync(orgId, "https://example.com/billing", Arg.Any<CancellationToken>())
            .Returns("https://billing.stripe.com/portal123");

        var command = new CreateCustomerPortalSessionCommand(orgId, userId, "https://example.com/billing");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.PortalUrl.Should().Be("https://billing.stripe.com/portal123");
    }

    [Fact]
    public async Task Handle_WithAdmin_ShouldReturnPortalUrl()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var org = Organization.Create("Test Org");
        var membership = Membership.Create(userId, orgId, MembershipRole.Admin);

        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>()).Returns(org);
        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>()).Returns(membership);
        _paymentService.IsEnabled.Returns(true);
        _paymentService.CreateCustomerPortalSessionAsync(orgId, "https://example.com/billing", Arg.Any<CancellationToken>())
            .Returns("https://billing.stripe.com/portal456");

        var command = new CreateCustomerPortalSessionCommand(orgId, userId, "https://example.com/billing");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.PortalUrl.Should().Be("https://billing.stripe.com/portal456");
    }

    [Fact]
    public async Task Handle_WhenOrgNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _organizationRepository.GetByIdAsync(orgId, Arg.Any<CancellationToken>()).Returns((Organization?)null);

        var command = new CreateCustomerPortalSessionCommand(orgId, userId, "https://example.com/billing");

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

        var command = new CreateCustomerPortalSessionCommand(orgId, userId, "https://example.com/billing");

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

        var command = new CreateCustomerPortalSessionCommand(orgId, userId, "https://example.com/billing");

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

        var command = new CreateCustomerPortalSessionCommand(orgId, userId, "https://example.com/billing");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();
    }
}
