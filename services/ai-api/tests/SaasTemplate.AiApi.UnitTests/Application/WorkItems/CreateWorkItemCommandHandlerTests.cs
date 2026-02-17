// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.AiApi.Application.Common.Interfaces;
using SaasTemplate.AiApi.Application.Common.Telemetry;
using SaasTemplate.AiApi.Application.WorkItems.Commands;
using SaasTemplate.AiApi.Application.WorkItems.Interfaces;
using SaasTemplate.AiApi.Domain.WorkItems;
using SaasTemplate.AiApi.Domain.WorkItems.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace SaasTemplate.AiApi.UnitTests.Application.WorkItems;

public sealed class CreateWorkItemCommandHandlerTests
{
    private readonly IWorkItemRepository _workItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateWorkItemCommandHandler _handler;

    public CreateWorkItemCommandHandlerTests()
    {
        _workItemRepository = Substitute.For<IWorkItemRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreateWorkItemCommandHandler(
            _workItemRepository,
            _unitOfWork,
            new WorkItemMetrics());
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateWorkItem()
    {
        // Arrange
        var command = new CreateWorkItemCommand(Guid.NewGuid(), Guid.NewGuid(), "Test Title", "Test Description");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Title.Should().Be("Test Title");
        result.Description.Should().Be("Test Description");
        result.Status.Should().Be(WorkItemStatus.Pending);
        result.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.UpdatedAtUtc.Should().Be(result.CreatedAtUtc);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPersistWorkItem()
    {
        // Arrange
        var command = new CreateWorkItemCommand(Guid.NewGuid(), Guid.NewGuid(), "Test Title");
        WorkItem? capturedWorkItem = null;

        _workItemRepository
            .AddAsync(Arg.Do<WorkItem>(w => capturedWorkItem = w), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _workItemRepository.Received(1).AddAsync(Arg.Any<WorkItem>(), Arg.Any<CancellationToken>());
        capturedWorkItem.Should().NotBeNull();
        capturedWorkItem!.Title.Should().Be("Test Title");
        capturedWorkItem.Status.Should().Be(WorkItemStatus.Pending);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSaveChanges()
    {
        // Arrange
        var command = new CreateWorkItemCommand(Guid.NewGuid(), Guid.NewGuid(), "Test Title");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyTitle_ShouldThrowValidationException()
    {
        // Arrange
        var command = new CreateWorkItemCommand(Guid.NewGuid(), Guid.NewGuid(), "");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<WorkItemValidationException>()
            .Where(e => e.PropertyName == "Title");
    }

    [Fact]
    public async Task Handle_WithTitleExceedingMaxLength_ShouldThrowValidationException()
    {
        // Arrange
        var longTitle = new string('a', WorkItem.TitleMaxLength + 1);
        var command = new CreateWorkItemCommand(Guid.NewGuid(), Guid.NewGuid(), longTitle);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<WorkItemValidationException>()
            .Where(e => e.PropertyName == "Title");
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassItToRepositories()
    {
        // Arrange
        var command = new CreateWorkItemCommand(Guid.NewGuid(), Guid.NewGuid(), "Test Title");
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        await _handler.Handle(command, token);

        // Assert
        await _workItemRepository.Received(1).AddAsync(Arg.Any<WorkItem>(), token);
        await _unitOfWork.Received(1).SaveChangesAsync(token);
    }

    [Fact]
    public async Task Handle_ShouldReturnDataMatchingPersistedWorkItem()
    {
        // Arrange
        var command = new CreateWorkItemCommand(Guid.NewGuid(), Guid.NewGuid(), "Test Title", "Test Description");
        WorkItem? capturedWorkItem = null;

        _workItemRepository
            .AddAsync(Arg.Do<WorkItem>(w => capturedWorkItem = w), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedWorkItem.Should().NotBeNull();
        result.Id.Should().Be(capturedWorkItem!.Id);
        result.Title.Should().Be(capturedWorkItem.Title);
        result.Description.Should().Be(capturedWorkItem.Description);
        result.Status.Should().Be(capturedWorkItem.Status);
        result.CreatedAtUtc.Should().Be(capturedWorkItem.CreatedAtUtc);
        result.UpdatedAtUtc.Should().Be(capturedWorkItem.UpdatedAtUtc);
    }

    [Fact]
    public async Task Handle_WithValidCommandAndDescription_ShouldPersistWorkItemWithDescription()
    {
        // Arrange
        var command = new CreateWorkItemCommand(Guid.NewGuid(), Guid.NewGuid(), "Test Title", "Test Description");
        WorkItem? capturedWorkItem = null;

        _workItemRepository
            .AddAsync(Arg.Do<WorkItem>(w => capturedWorkItem = w), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedWorkItem.Should().NotBeNull();
        capturedWorkItem!.Description.Should().Be("Test Description");
    }
}
