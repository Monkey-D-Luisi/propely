// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.AiApi.Domain.Common;
using SaasTemplate.AiApi.Domain.WorkItems;
using SaasTemplate.AiApi.Domain.WorkItems.Events;
using SaasTemplate.AiApi.Domain.WorkItems.Exceptions;
using FluentAssertions;

namespace SaasTemplate.AiApi.UnitTests.Domain.WorkItems;

public class WorkItemTests
{
    [Fact]
    public void Create_WithValidTitle_ShouldCreateWorkItem()
    {
        // Arrange
        var title = "Valid Title";

        // Act
        var workItem = WorkItem.Create(Guid.NewGuid(), Guid.NewGuid(), title);

        // Assert
        workItem.Id.Should().NotBeEmpty();
        workItem.Title.Should().Be(title);
        workItem.Description.Should().BeNull();
        workItem.Status.Should().Be(WorkItemStatus.Pending);
        workItem.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        workItem.UpdatedAtUtc.Should().Be(workItem.CreatedAtUtc);
        workItem.Version.Should().Be(1);
    }

    [Fact]
    public void Create_WithValidTitleAndDescription_ShouldCreateWorkItem()
    {
        // Arrange
        var title = "Valid Title";
        var description = "Valid Description";

        // Act
        var workItem = WorkItem.Create(Guid.NewGuid(), Guid.NewGuid(), title, description);

        // Assert
        workItem.Title.Should().Be(title);
        workItem.Description.Should().Be(description);
    }

    [Fact]
    public void Create_WithMaxLengthTitle_ShouldCreateWorkItem()
    {
        // Arrange
        var title = new string('a', WorkItem.TitleMaxLength);

        // Act
        var workItem = WorkItem.Create(Guid.NewGuid(), Guid.NewGuid(), title);

        // Assert
        workItem.Title.Should().HaveLength(WorkItem.TitleMaxLength);
    }

    [Fact]
    public void Create_WithMaxLengthDescription_ShouldCreateWorkItem()
    {
        // Arrange
        var title = "Valid Title";
        var description = new string('a', WorkItem.DescriptionMaxLength);

        // Act
        var workItem = WorkItem.Create(Guid.NewGuid(), Guid.NewGuid(), title, description);

        // Assert
        workItem.Description.Should().HaveLength(WorkItem.DescriptionMaxLength);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespaceTitle_ShouldThrowValidationException(string? title)
    {
        // Act
        var act = () => WorkItem.Create(Guid.NewGuid(), Guid.NewGuid(), title!);

        // Assert
        act.Should().Throw<WorkItemValidationException>()
            .WithMessage("Title is required and cannot be empty.")
            .And.PropertyName.Should().Be("Title");
    }

    [Fact]
    public void Create_WithTitleExceedingMaxLength_ShouldThrowValidationException()
    {
        // Arrange
        var title = new string('a', WorkItem.TitleMaxLength + 1);

        // Act
        var act = () => WorkItem.Create(Guid.NewGuid(), Guid.NewGuid(), title);

        // Assert
        act.Should().Throw<WorkItemValidationException>()
            .WithMessage($"Title cannot exceed {WorkItem.TitleMaxLength} characters.")
            .And.PropertyName.Should().Be("Title");
    }

    [Fact]
    public void Create_WithDescriptionExceedingMaxLength_ShouldThrowValidationException()
    {
        // Arrange
        var title = "Valid Title";
        var description = new string('a', WorkItem.DescriptionMaxLength + 1);

        // Act
        var act = () => WorkItem.Create(Guid.NewGuid(), Guid.NewGuid(), title, description);

        // Assert
        act.Should().Throw<WorkItemValidationException>()
            .WithMessage($"Description cannot exceed {WorkItem.DescriptionMaxLength} characters.")
            .And.PropertyName.Should().Be("Description");
    }

    [Fact]
    public void Create_ShouldRaiseWorkItemCreatedEvent()
    {
        // Arrange
        var title = "Test Work Item";
        var description = "Test Description";
        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();

        // Act
        var workItem = WorkItem.Create(Guid.NewGuid(), Guid.NewGuid(), title, description, correlationId: correlationId, causationId: causationId);

        // Assert
        workItem.DomainEvents.Should().ContainSingle();
        var domainEvent = workItem.DomainEvents.Single();
        domainEvent.Should().BeOfType<WorkItemCreatedV1>();

        var createdEvent = (WorkItemCreatedV1)domainEvent;
        createdEvent.EventId.Should().NotBeEmpty();
        createdEvent.EventType.Should().Be("WorkItemCreatedV1");
        createdEvent.SchemaVersion.Should().Be(1);
        createdEvent.Producer.Should().Be("WorkItemApi");
        createdEvent.CorrelationId.Should().Be(correlationId);
        createdEvent.CausationId.Should().Be(causationId);

        createdEvent.Data.WorkItemId.Should().Be(workItem.Id);
        createdEvent.Data.Title.Should().Be(title);
        createdEvent.Data.Description.Should().Be(description);
        createdEvent.Data.Status.Should().Be(WorkItemStatus.Pending);
        createdEvent.Data.CreatedAtUtc.Should().Be(workItem.CreatedAtUtc);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var workItem = WorkItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Test");

        // Act
        workItem.ClearDomainEvents();

        // Assert
        workItem.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithTitleWithWhitespace_ShouldTrimTitle()
    {
        // Arrange
        var title = "  Trimmed Title  ";

        // Act
        var workItem = WorkItem.Create(Guid.NewGuid(), Guid.NewGuid(), title);

        // Assert
        workItem.Title.Should().Be("Trimmed Title");
    }

    [Fact]
    public void Create_WithDescriptionWithWhitespace_ShouldTrimDescription()
    {
        // Arrange
        var title = "Valid Title";
        var description = "  Trimmed Description  ";

        // Act
        var workItem = WorkItem.Create(Guid.NewGuid(), Guid.NewGuid(), title, description);

        // Assert
        workItem.Description.Should().Be("Trimmed Description");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespaceDescription_ShouldNormalizeToNull(string description)
    {
        // Arrange
        var title = "Valid Title";

        // Act
        var workItem = WorkItem.Create(Guid.NewGuid(), Guid.NewGuid(), title, description);

        // Assert
        workItem.Description.Should().BeNull();
    }

    [Fact]
    public void Create_EventOccurredAtUtc_ShouldMatchCreatedAtUtc()
    {
        // Arrange & Act
        var workItem = WorkItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Test");
        var createdEvent = (WorkItemCreatedV1)workItem.DomainEvents.Single();

        // Assert
        createdEvent.OccurredAtUtc.Should().Be(workItem.CreatedAtUtc);
    }

    [Fact]
    public void WorkItem_ShouldImplementISoftDeletable()
    {
        // Assert
        typeof(WorkItem).Should().Implement<ISoftDeletable>();
    }

    [Fact]
    public void Create_ShouldNotBeDeleted()
    {
        // Act
        var workItem = WorkItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Test");

        // Assert
        workItem.IsDeleted.Should().BeFalse();
        workItem.DeletedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Delete_ShouldSetIsDeletedToTrue()
    {
        // Arrange
        var workItem = WorkItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Test");
        workItem.ClearDomainEvents();

        // Act
        workItem.Delete();

        // Assert
        workItem.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Delete_ShouldSetDeletedAtUtc()
    {
        // Arrange
        var workItem = WorkItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Test");
        workItem.ClearDomainEvents();

        // Act
        workItem.Delete();

        // Assert
        workItem.DeletedAtUtc.Should().NotBeNull();
        workItem.DeletedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Delete_ShouldSetStatusToDeleted()
    {
        // Arrange
        var workItem = WorkItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Test");
        workItem.ClearDomainEvents();

        // Act
        workItem.Delete();

        // Assert
        workItem.Status.Should().Be(WorkItemStatus.Deleted);
    }

    [Fact]
    public void SoftDelete_ShouldDelegateToDelete()
    {
        // Arrange
        var workItem = WorkItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Test");
        workItem.ClearDomainEvents();

        // Act
        workItem.SoftDelete();

        // Assert
        workItem.IsDeleted.Should().BeTrue();
        workItem.DeletedAtUtc.Should().NotBeNull();
        workItem.Status.Should().Be(WorkItemStatus.Deleted);
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_ShouldNotChangeDeletedAtUtc()
    {
        // Arrange
        var workItem = WorkItem.Create(Guid.NewGuid(), Guid.NewGuid(), "Test");
        workItem.Delete();
        var firstDeletedAt = workItem.DeletedAtUtc;

        // Act
        workItem.Delete();

        // Assert
        workItem.DeletedAtUtc.Should().Be(firstDeletedAt);
    }
}
