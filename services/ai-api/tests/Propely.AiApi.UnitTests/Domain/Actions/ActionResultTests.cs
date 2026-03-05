// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.UnitTests.Domain.Actions;

public sealed class ActionResultTests
{
    [Fact]
    public void Ok_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = ActionResult.Ok("test data", "Success message", ActionType.CreateProperty, 0.95);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().Be("test data");
        result.Message.Should().Be("Success message");
        result.ActionType.Should().Be(ActionType.CreateProperty);
        result.Confidence.Should().Be(0.95);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Fail_ShouldCreateFailedResult()
    {
        // Act
        var result = ActionResult.Fail(
            ["Error 1", "Error 2"],
            ActionType.QueryProperties,
            "Something went wrong");

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Message.Should().Be("Something went wrong");
        result.ActionType.Should().Be(ActionType.QueryProperties);
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain("Error 1");
    }

    [Fact]
    public void GenericOk_ShouldCreateSuccessfulTypedResult()
    {
        // Act
        var result = ActionResult<string>.Ok("typed data", "Success", ActionType.GenerateCopy);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().Be("typed data");
        result.ActionType.Should().Be(ActionType.GenerateCopy);
        result.Confidence.Should().Be(1.0);
    }

    [Fact]
    public void GenericFail_ShouldCreateFailedTypedResult()
    {
        // Act
        var result = ActionResult<string>.Fail(["Validation error"], ActionType.CreateProperty, "Invalid input");

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Errors.Should().ContainSingle("Validation error");
        result.Message.Should().Be("Invalid input");
    }

    [Fact]
    public void ClassifiedIntent_ShouldStoreAllProperties()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["city"] = "Barcelona",
            ["price"] = 300000.0
        };

        // Act
        var intent = new ClassifiedIntent(ActionType.QueryProperties, parameters, 0.85, "query_properties");

        // Assert
        intent.ActionType.Should().Be(ActionType.QueryProperties);
        intent.Parameters.Should().HaveCount(2);
        intent.Parameters["city"].Should().Be("Barcelona");
        intent.Confidence.Should().Be(0.85);
        intent.RawFunctionName.Should().Be("query_properties");
    }

    [Fact]
    public void ClassifiedIntent_WhenNoRawFunctionName_ShouldDefaultToNull()
    {
        // Act
        var intent = new ClassifiedIntent(ActionType.Unknown, new Dictionary<string, object?>(), 0.0);

        // Assert
        intent.RawFunctionName.Should().BeNull();
    }
}
