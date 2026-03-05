// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using FluentValidation.TestHelper;
using Propely.PropertiesApi.Application.Properties.Commands.CreateProperty;
using Propely.PropertiesApi.Application.Properties.Dtos;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.UnitTests.Properties.Commands;

public class CreatePropertyCommandValidatorTests
{
    private readonly CreatePropertyCommandValidator _validator = new();

    private static CreatePropertyCommand ValidCommand() => new()
    {
        Title = "Valid Title",
        PropertyType = PropertyType.Apartment,
        OperationType = OperationType.Sale,
        TenantId = Guid.NewGuid(),
        AgentId = Guid.NewGuid()
    };

    [Fact]
    public void ValidCommand_PassesValidation()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void EmptyTitle_FailsValidation(string? title)
    {
        var command = ValidCommand() with { Title = title! };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void TitleExceedsMaxLength_FailsValidation()
    {
        var command = ValidCommand() with { Title = new string('A', 201) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void EmptyTenantId_FailsValidation()
    {
        var command = ValidCommand() with { TenantId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void EmptyAgentId_FailsValidation()
    {
        var command = ValidCommand() with { AgentId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.AgentId);
    }

    [Fact]
    public void NegativePrice_FailsValidation()
    {
        var command = ValidCommand() with { Financials = new PropertyFinancialsDto { Price = -1 } };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Financials!.Price);
    }

    [Fact]
    public void ZeroPrice_PassesValidation()
    {
        var command = ValidCommand() with { Financials = new PropertyFinancialsDto { Price = 0 } };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
