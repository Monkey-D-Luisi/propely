// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Models;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Application.Organizations.Queries.GetMyOrgs;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Organizations.Queries;

public sealed class GetMyOrgsQueryHandlerTests
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly GetMyOrgsQueryHandler _handler;

    public GetMyOrgsQueryHandlerTests()
    {
        _membershipRepository = Substitute.For<IMembershipRepository>();
        _handler = new GetMyOrgsQueryHandler(_membershipRepository);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnPagedResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetMyOrgsQuery(userId, 1, 20);
        var expectedResult = new PagedResult<OrgWithRole>(
            new[] { new OrgWithRole(Guid.NewGuid(), "Org 1", null, "Owner") },
            1, 1, 20);

        _membershipRepository.GetOrgsPagedAsync(userId, 1, 20, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithPageLessThanOne_ShouldClampToOne()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetMyOrgsQuery(userId, -5, 20);
        var expectedResult = new PagedResult<OrgWithRole>([], 0, 1, 20);

        _membershipRepository.GetOrgsPagedAsync(userId, 1, 20, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _membershipRepository.Received(1).GetOrgsPagedAsync(userId, 1, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithPageSizeLessThanOne_ShouldClampToOne()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetMyOrgsQuery(userId, 1, 0);
        var expectedResult = new PagedResult<OrgWithRole>([], 0, 1, 1);

        _membershipRepository.GetOrgsPagedAsync(userId, 1, 1, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _membershipRepository.Received(1).GetOrgsPagedAsync(userId, 1, 1, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithPageSizeOverOneHundred_ShouldClampToOneHundred()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetMyOrgsQuery(userId, 1, 500);
        var expectedResult = new PagedResult<OrgWithRole>([], 0, 1, 100);

        _membershipRepository.GetOrgsPagedAsync(userId, 1, 100, Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _membershipRepository.Received(1).GetOrgsPagedAsync(userId, 1, 100, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetMyOrgsQuery(userId, 1, 20);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _membershipRepository.GetOrgsPagedAsync(userId, 1, 20, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<OrgWithRole>([], 0, 1, 20));

        // Act
        await _handler.Handle(query, token);

        // Assert
        await _membershipRepository.Received(1).GetOrgsPagedAsync(userId, 1, 20, token);
    }
}
