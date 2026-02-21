// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Agencies.Queries.ListAgenciesForUser;
using Propely.OrgsApi.Application.Agencies.Interfaces;
using Propely.OrgsApi.Domain.Agencies;
using FluentAssertions;
using NSubstitute;

namespace Propely.OrgsApi.UnitTests.Application.Agencies.Queries;

public sealed class ListAgenciesForUserQueryHandlerTests
{
    private readonly IAgencyRepository _agencyRepository;
    private readonly ListAgenciesForUserQueryHandler _handler;

    public ListAgenciesForUserQueryHandlerTests()
    {
        _agencyRepository = Substitute.For<IAgencyRepository>();
        _handler = new ListAgenciesForUserQueryHandler(_agencyRepository);
    }

    [Fact]
    public async Task Handle_WithAgencies_ShouldReturnMappedDtos()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var agency1 = Agency.Create("Agency One", "agency-one", userId);
        var agency2 = Agency.Create("Agency Two", "agency-two", userId);

        _agencyRepository.ListForUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Agency> { agency1, agency2 });

        var query = new ListAgenciesForUserQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Agency One");
        result[0].Slug.Should().Be("agency-one");
        result[0].BranchCount.Should().Be(0);
        result[1].Name.Should().Be("Agency Two");
        result[1].Slug.Should().Be("agency-two");
    }

    [Fact]
    public async Task Handle_WithNoAgencies_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _agencyRepository.ListForUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Agency>());

        var query = new ListAgenciesForUserQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldIncludeBranchCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var agency = Agency.Create("Agency", "agency-test", userId);
        agency.AddBranch(Guid.NewGuid());
        agency.AddBranch(Guid.NewGuid());

        _agencyRepository.ListForUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Agency> { agency });

        var query = new ListAgenciesForUserQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].BranchCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new ListAgenciesForUserQuery(userId);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _agencyRepository.ListForUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Agency>());

        // Act
        await _handler.Handle(query, token);

        // Assert
        await _agencyRepository.Received(1).ListForUserAsync(userId, token);
    }
}
