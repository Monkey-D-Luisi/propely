// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentValidation.TestHelper;
using Propely.PropertiesApi.Application.Properties.Commands.ChangeStatus;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.UnitTests.Properties.Commands;

public class ChangePropertyStatusCommandValidatorTests
{
    private readonly ChangePropertyStatusCommandValidator _validator = new();

    [Fact]
    public void ValidCommand_PassesValidation()
    {
        var command = new ChangePropertyStatusCommand(Guid.NewGuid(), Guid.NewGuid(), PropertyStatus.Active);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyPropertyId_FailsValidation()
    {
        var command = new ChangePropertyStatusCommand(Guid.Empty, Guid.NewGuid(), PropertyStatus.Active);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PropertyId);
    }

    [Fact]
    public void EmptyTenantId_FailsValidation()
    {
        var command = new ChangePropertyStatusCommand(Guid.NewGuid(), Guid.Empty, PropertyStatus.Active);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void InvalidStatus_FailsValidation()
    {
        var command = new ChangePropertyStatusCommand(Guid.NewGuid(), Guid.NewGuid(), (PropertyStatus)99);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.NewStatus);
    }
}
