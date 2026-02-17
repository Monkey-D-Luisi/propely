// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.AiApi.Application.WorkItems.Commands;
using SaasTemplate.AiApi.Application.Common.Interfaces;
using SaasTemplate.AiApi.Application.WorkItems.Interfaces;
using SaasTemplate.AiApi.Domain.WorkItems;
using SaasTemplate.AiApi.Domain.Common.Exceptions;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace SaasTemplate.AiApi.UnitTests.Application.WorkItems;

public class UpdateWorkItemCommandHandlerTests
{
    private readonly IWorkItemRepository _repositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly ILogger<UpdateWorkItemCommandHandler> _loggerMock;
    private readonly UpdateWorkItemCommandHandler _handler;

    public UpdateWorkItemCommandHandlerTests()
    {
        _repositoryMock = Substitute.For<IWorkItemRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _loggerMock = Substitute.For<ILogger<UpdateWorkItemCommandHandler>>();
        _handler = new UpdateWorkItemCommandHandler(_repositoryMock, _unitOfWorkMock, _loggerMock);
    }

    [Fact]
    public async Task Handle_ShouldUpdateWorkItem_WhenFound()
    {
        // Arrange
        var workItemId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var workItem = WorkItem.Create(Guid.NewGuid(), userId, "Original Title", "Original Description");
        
        _repositoryMock.GetByIdAsync(workItemId, Arg.Any<CancellationToken>())
            .Returns(workItem);

        var command = new UpdateWorkItemCommand(workItemId, userId, "New Title", "New Description", WorkItemStatus.Active);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        workItem.Title.Should().Be("New Title");
        workItem.Description.Should().Be("New Description");
        workItem.Status.Should().Be(WorkItemStatus.Active);

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

        var command = new UpdateWorkItemCommand(workItemId, Guid.NewGuid(), "Title", "Desc", WorkItemStatus.Active);

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

        var command = new UpdateWorkItemCommand(workItemId, otherUserId, "New Title", "New Desc", WorkItemStatus.Active);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
        await _repositoryMock.DidNotReceive().UpdateAsync(Arg.Any<WorkItem>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
