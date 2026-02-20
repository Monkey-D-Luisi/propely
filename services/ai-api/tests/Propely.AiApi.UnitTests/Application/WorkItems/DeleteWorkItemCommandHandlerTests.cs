// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Application.WorkItems.Commands;
using Propely.AiApi.Application.Common.Interfaces;
using Propely.AiApi.Application.WorkItems.Interfaces;
using Propely.AiApi.Domain.WorkItems;
using Propely.AiApi.Domain.Common.Exceptions;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Propely.AiApi.UnitTests.Application.WorkItems;

public class DeleteWorkItemCommandHandlerTests
{
    private readonly IWorkItemRepository _repositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly ILogger<DeleteWorkItemCommandHandler> _loggerMock;
    private readonly DeleteWorkItemCommandHandler _handler;

    public DeleteWorkItemCommandHandlerTests()
    {
        _repositoryMock = Substitute.For<IWorkItemRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _loggerMock = Substitute.For<ILogger<DeleteWorkItemCommandHandler>>();
        _handler = new DeleteWorkItemCommandHandler(_repositoryMock, _unitOfWorkMock, _loggerMock);
    }

    [Fact]
    public async Task Handle_ShouldDeleteWorkItem_WhenFound()
    {
        // Arrange
        var workItemId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var workItem = WorkItem.Create(Guid.NewGuid(), userId, "Title", "Description");
        
        _repositoryMock.GetByIdAsync(workItemId, Arg.Any<CancellationToken>())
            .Returns(workItem);

        var command = new DeleteWorkItemCommand(workItemId, userId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        workItem.Status.Should().Be(WorkItemStatus.Deleted);

        await _repositoryMock.Received(1).UpdateAsync(workItem, Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenNotFound()
    {
        // Arrange
        var workItemId = Guid.NewGuid();
        _repositoryMock.GetByIdAsync(workItemId, Arg.Any<CancellationToken>())
            .Returns((WorkItem?)null);

        var command = new DeleteWorkItemCommand(workItemId, Guid.NewGuid());

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        await _repositoryMock.DidNotReceive().UpdateAsync(Arg.Any<WorkItem>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenUserNotOwner()
    {
        // Arrange
        var workItemId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var workItem = WorkItem.Create(Guid.NewGuid(), ownerId, "Title", "Desc");
        
        _repositoryMock.GetByIdAsync(workItemId, Arg.Any<CancellationToken>())
            .Returns(workItem);

        var command = new DeleteWorkItemCommand(workItemId, otherUserId);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
        await _repositoryMock.DidNotReceive().UpdateAsync(Arg.Any<WorkItem>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
