// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.ContactsApi.Application.Leads.Commands.CreateLead;

namespace Propely.ContactsApi.UnitTests.Application.Leads.Commands;

public class CreateLeadCommandValidatorTests
{
    private readonly CreateLeadCommandValidator _validator = new();

    private static CreateLeadCommand ValidCommand() => new()
    {
        Name = "Pedro Gómez",
        Email = "pedro@example.com",
        PropertyId = Guid.NewGuid(),
        TenantId = Guid.NewGuid()
    };

    [Fact]
    public void Validate_ValidCommand_PassesWithNoErrors()
    {
        var result = _validator.Validate(ValidCommand());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyName_FailsValidation()
    {
        var cmd = ValidCommand() with { Name = "" };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.Name));
    }

    [Fact]
    public void Validate_InvalidEmail_FailsValidation()
    {
        var cmd = ValidCommand() with { Email = "bad-email" };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.Email));
    }

    [Fact]
    public void Validate_EmptyPropertyId_FailsValidation()
    {
        var cmd = ValidCommand() with { PropertyId = Guid.Empty };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.PropertyId));
    }

    [Fact]
    public void Validate_EmptyTenantId_FailsValidation()
    {
        var cmd = ValidCommand() with { TenantId = Guid.Empty };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.TenantId));
    }
}
