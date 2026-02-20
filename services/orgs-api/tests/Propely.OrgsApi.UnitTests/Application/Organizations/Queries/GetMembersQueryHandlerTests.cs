// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Models;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Application.Organizations.Queries.GetMembers;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Organizations;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Organizations.Queries;

public sealed class GetMembersQueryHandlerTests
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly GetMembersQueryHandler _handler;

    public GetMembersQueryHandlerTests()
    {
        _membershipRepository = Substitute.For<IMembershipRepository>();
        _handler = new GetMembersQueryHandler(_membershipRepository);
    }

    [Fact]
    public async Task Handle_WhenUserIsMember_ShouldReturnPagedResult()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Member);
        var query = new GetMembersQuery(orgId, userId, 1, 20);
        var expectedResult = new PagedResult<MemberDto>(
            new[] { new MemberDto(userId, "test@example.com", "John Doe", "Member") },
            1, 1, 20);

        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>())
            .Returns(membership);
        _membershipRepository.GetMembersPagedAsync(orgId, 1, 20, null, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotMember_ShouldThrowForbiddenException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var query = new GetMembersQuery(orgId, userId, 1, 20);

        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>())
            .Returns((Membership?)null);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("User is not a member of this organization.");
    }

    [Fact]
    public async Task Handle_WhenUserIsNotMember_ShouldNotQueryMembers()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var query = new GetMembersQuery(orgId, userId, 1, 20);

        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>())
            .Returns((Membership?)null);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
        await _membershipRepository.DidNotReceive().GetMembersPagedAsync(
            Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithSearchParameter_ShouldPassSearchToRepository()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Owner);
        var query = new GetMembersQuery(orgId, userId, 1, 20, "john");

        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>())
            .Returns(membership);
        _membershipRepository.GetMembersPagedAsync(orgId, 1, 20, "john", Arg.Any<CancellationToken>())
            .Returns(new PagedResult<MemberDto>([], 0, 1, 20));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _membershipRepository.Received(1).GetMembersPagedAsync(orgId, 1, 20, "john", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithPageLessThanOne_ShouldClampToOne()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Member);
        var query = new GetMembersQuery(orgId, userId, -3, 20);

        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>())
            .Returns(membership);
        _membershipRepository.GetMembersPagedAsync(orgId, 1, 20, null, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<MemberDto>([], 0, 1, 20));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _membershipRepository.Received(1).GetMembersPagedAsync(orgId, 1, 20, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithPageSizeOverOneHundred_ShouldClampToOneHundred()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Member);
        var query = new GetMembersQuery(orgId, userId, 1, 200);

        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>())
            .Returns(membership);
        _membershipRepository.GetMembersPagedAsync(orgId, 1, 100, null, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<MemberDto>([], 0, 1, 100));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _membershipRepository.Received(1).GetMembersPagedAsync(orgId, 1, 100, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToAllDependencies()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var membership = Membership.Create(userId, orgId, MembershipRole.Member);
        var query = new GetMembersQuery(orgId, userId, 1, 20);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _membershipRepository.GetAsync(orgId, userId, Arg.Any<CancellationToken>())
            .Returns(membership);
        _membershipRepository.GetMembersPagedAsync(orgId, 1, 20, null, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<MemberDto>([], 0, 1, 20));

        // Act
        await _handler.Handle(query, token);

        // Assert
        await _membershipRepository.Received(1).GetAsync(orgId, userId, token);
        await _membershipRepository.Received(1).GetMembersPagedAsync(orgId, 1, 20, null, token);
    }
}
