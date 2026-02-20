// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Billing.Interfaces;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Organizations.Commands.CreateOrganization;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Organizations;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Organizations.Commands;

public sealed class CreateOrganizationCommandHandlerTests
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEntitlementService _entitlementService;
    private readonly CreateOrganizationCommandHandler _handler;

    public CreateOrganizationCommandHandlerTests()
    {
        _organizationRepository = Substitute.For<IOrganizationRepository>();
        _membershipRepository = Substitute.For<IMembershipRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _entitlementService = Substitute.For<IEntitlementService>();
        _entitlementService.CanCreateOrganizationAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _handler = new CreateOrganizationCommandHandler(
            _organizationRepository, _membershipRepository, _unitOfWork, _entitlementService);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnOrgIdAndName()
    {
        // Arrange
        var command = new CreateOrganizationCommand("My Organization", Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.OrgId.Should().NotBeEmpty();
        result.Name.Should().Be("My Organization");
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPersistOrganization()
    {
        // Arrange
        var command = new CreateOrganizationCommand("My Organization", Guid.NewGuid());
        Organization? capturedOrg = null;

        _organizationRepository
            .AddAsync(Arg.Do<Organization>(o => capturedOrg = o), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _organizationRepository.Received(1).AddAsync(Arg.Any<Organization>(), Arg.Any<CancellationToken>());
        capturedOrg.Should().NotBeNull();
        capturedOrg!.Name.Should().Be("My Organization");
    }

    [Fact]
    public async Task Handle_ShouldCreateOwnerMembership()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var command = new CreateOrganizationCommand("My Organization", ownerId);
        Membership? capturedMembership = null;

        _membershipRepository
            .AddAsync(Arg.Do<Membership>(m => capturedMembership = m), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _membershipRepository.Received(1).AddAsync(Arg.Any<Membership>(), Arg.Any<CancellationToken>());
        capturedMembership.Should().NotBeNull();
        capturedMembership!.UserId.Should().Be(ownerId);
        capturedMembership.OrganizationId.Should().Be(result.OrgId);
        capturedMembership.Role.Should().Be(MembershipRole.Owner);
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
    {
        // Arrange
        var command = new CreateOrganizationCommand("My Organization", Guid.NewGuid());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToAllDependencies()
    {
        // Arrange
        var command = new CreateOrganizationCommand("My Organization", Guid.NewGuid());
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        await _handler.Handle(command, token);

        // Assert
        await _organizationRepository.Received(1).AddAsync(Arg.Any<Organization>(), token);
        await _membershipRepository.Received(1).AddAsync(Arg.Any<Membership>(), token);
        await _unitOfWork.Received(1).SaveChangesAsync(token);
    }

    [Fact]
    public async Task Handle_WithTrimmedName_ShouldReturnTrimmedName()
    {
        // Arrange
        var command = new CreateOrganizationCommand("  My Organization  ", Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("My Organization");
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var command = new CreateOrganizationCommand("Acme Corp", Guid.NewGuid());
        _organizationRepository.ExistsByNameAsync("Acme Corp", null, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_WhenNameIsUnique_ShouldNotThrow()
    {
        // Arrange
        var command = new CreateOrganizationCommand("Unique Corp", Guid.NewGuid());
        _organizationRepository.ExistsByNameAsync("Unique Corp", null, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("Unique Corp");
    }

    [Fact]
    public async Task Handle_WhenOrgLimitReached_ShouldThrowForbiddenException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateOrganizationCommand("New Org", userId);
        _entitlementService.CanCreateOrganizationAsync(userId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Organization limit reached for your current plan.");
        await _organizationRepository.DidNotReceive().AddAsync(Arg.Any<Organization>(), Arg.Any<CancellationToken>());
    }
}
