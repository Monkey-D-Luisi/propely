// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.AiApi.Application.Common.Models;
using SaasTemplate.AiApi.Application.WorkItems.Dtos;
using SaasTemplate.AiApi.Application.WorkItems.Interfaces;
using SaasTemplate.AiApi.Application.WorkItems.Queries.ListWorkItems;
using SaasTemplate.AiApi.Domain.WorkItems;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace SaasTemplate.AiApi.UnitTests.Application.WorkItems.Queries.ListWorkItems;

public class ListWorkItemsQueryHandlerTests
{
    private readonly IWorkItemReadRepository _repositoryMock;
    private readonly ListWorkItemsQueryHandler _handler;

    public ListWorkItemsQueryHandlerTests()
    {
        _repositoryMock = Substitute.For<IWorkItemReadRepository>();
        _handler = new ListWorkItemsQueryHandler(_repositoryMock);
    }

    [Fact]
    public async Task Handle_ShouldCallRepository_WithCorrectParameters()
    {
        // Arrange
        var query = new ListWorkItemsQuery(Page: 2, PageSize: 5, Status: WorkItemStatus.Active);
        var pagedResult = new PagedResult<WorkItemDto>(Enumerable.Empty<WorkItemDto>(), 0, 2, 5);

        _repositoryMock.ListAsync(2, 5, WorkItemStatus.Active, null, Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(pagedResult);
        await _repositoryMock.Received(1).ListAsync(2, 5, WorkItemStatus.Active, null, Arg.Any<CancellationToken>());
    }
}
