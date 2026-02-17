// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.AiApi.Domain.WorkItems;
using FluentAssertions;

namespace SaasTemplate.AiApi.UnitTests.Domain.WorkItems;

public class WorkItemStatusTests
{
    [Fact]
    public void WorkItemStatus_ShouldHaveExactlyThreeValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<WorkItemStatus>();

        // Assert
        values.Should().HaveCount(5);
    }

    [Theory]
    [InlineData(WorkItemStatus.Pending, 0)]
    [InlineData(WorkItemStatus.Active, 1)]
    [InlineData(WorkItemStatus.Deleted, 2)]
    [InlineData(WorkItemStatus.Deactivated, 3)]
    [InlineData(WorkItemStatus.Expired, 4)]
    public void WorkItemStatus_ShouldHaveCorrectValues(WorkItemStatus status, int expectedValue)
    {
        // Assert
        ((int)status).Should().Be(expectedValue);
    }

    [Fact]
    public void WorkItemStatus_ShouldContainPending()
    {
        Enum.IsDefined(WorkItemStatus.Pending).Should().BeTrue();
    }

    [Fact]
    public void WorkItemStatus_ShouldContainActive()
    {
        Enum.IsDefined(WorkItemStatus.Active).Should().BeTrue();
    }

    [Fact]
    public void WorkItemStatus_ShouldContainDeleted()
    {
        Enum.IsDefined(WorkItemStatus.Deleted).Should().BeTrue();
    }
}
