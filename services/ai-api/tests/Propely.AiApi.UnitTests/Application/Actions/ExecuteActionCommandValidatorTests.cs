// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.AiApi.Application.Actions.Commands.ExecuteAction;

namespace Propely.AiApi.UnitTests.Application.Actions;

public sealed class ExecuteActionCommandValidatorTests
{
    private readonly ExecuteActionCommandValidator _validator = new();

    [Fact]
    public async Task Validate_WhenTextIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var command = new ExecuteActionCommand("", Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Text");
    }

    [Fact]
    public async Task Validate_WhenTextExceedsMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longText = new string('a', 2001);
        var command = new ExecuteActionCommand(longText, Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Text");
    }

    [Fact]
    public async Task Validate_WhenTenantIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var command = new ExecuteActionCommand("create a property", Guid.Empty, Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TenantId");
    }

    [Fact]
    public async Task Validate_WhenAgentIdIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var command = new ExecuteActionCommand("create a property", Guid.NewGuid(), Guid.Empty);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AgentId");
    }

    [Fact]
    public async Task Validate_WhenAllFieldsValid_ShouldPass()
    {
        // Arrange
        var command = new ExecuteActionCommand("Create a 3 bedroom apartment in Malaga", Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenTextIsAtMaxLength_ShouldPass()
    {
        // Arrange
        var text = new string('a', 2000);
        var command = new ExecuteActionCommand(text, Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
