// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using FluentValidation.TestHelper;
using Propely.PropertiesApi.Application.Properties.Commands.UpdateProperty;
using Propely.PropertiesApi.Application.Properties.Dtos;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.UnitTests.Properties.Commands;

public class UpdatePropertyCommandValidatorTests
{
    private readonly UpdatePropertyCommandValidator _validator = new();

    private static UpdatePropertyCommand ValidCommand() => new()
    {
        PropertyId = Guid.NewGuid(),
        TenantId = Guid.NewGuid(),
        UpdatedBy = Guid.NewGuid(),
        Title = "Updated Title"
    };

    [Fact]
    public void ValidCommand_PassesValidation()
    {
        var result = _validator.TestValidate(ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyPropertyId_FailsValidation()
    {
        var command = ValidCommand() with { PropertyId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PropertyId);
    }

    [Fact]
    public void EmptyTenantId_FailsValidation()
    {
        var command = ValidCommand() with { TenantId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void EmptyTitle_WhenProvided_FailsValidation()
    {
        var command = ValidCommand() with { Title = "" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void NullTitle_PassesValidation()
    {
        var command = ValidCommand() with { Title = null };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void TitleExceedsMaxLength_FailsValidation()
    {
        var command = ValidCommand() with { Title = new string('a', Property.TitleMaxLength + 1) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void InvalidPropertyType_FailsValidation()
    {
        var command = ValidCommand() with { PropertyType = (PropertyType)99 };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PropertyType!.Value);
    }

    [Fact]
    public void InvalidOperationType_FailsValidation()
    {
        var command = ValidCommand() with { OperationType = (OperationType)99 };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.OperationType!.Value);
    }

    [Fact]
    public void NegativePrice_FailsValidation()
    {
        var command = ValidCommand() with
        {
            Financials = new PropertyFinancialsDto { Price = -1 }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Financials!.Price);
    }

    [Fact]
    public void ValidPrice_PassesValidation()
    {
        var command = ValidCommand() with
        {
            Financials = new PropertyFinancialsDto { Price = 100000 }
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Financials!.Price);
    }
}
