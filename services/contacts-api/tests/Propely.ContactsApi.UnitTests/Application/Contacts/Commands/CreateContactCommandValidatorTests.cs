// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.ContactsApi.Application.Contacts.Commands.CreateContact;
using Propely.ContactsApi.Domain.Contacts;

namespace Propely.ContactsApi.UnitTests.Application.Contacts.Commands;

public class CreateContactCommandValidatorTests
{
    private readonly CreateContactCommandValidator _validator = new();

    private static CreateContactCommand ValidCommand() => new()
    {
        FirstName = "Ana",
        LastName = "Lopez",
        Email = "ana@example.com",
        TenantId = Guid.NewGuid(),
        Roles = [ContactRole.Buyer]
    };

    [Fact]
    public void Validate_ValidCommand_PassesWithNoErrors()
    {
        var result = _validator.Validate(ValidCommand());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyFirstName_FailsValidation()
    {
        var cmd = ValidCommand() with { FirstName = "" };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.FirstName));
    }

    [Fact]
    public void Validate_InvalidEmail_FailsValidation()
    {
        var cmd = ValidCommand() with { Email = "not-an-email" };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.Email));
    }

    [Fact]
    public void Validate_EmptyRoles_FailsWithCustomMessage()
    {
        var cmd = ValidCommand() with { Roles = [] };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.Roles));
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
