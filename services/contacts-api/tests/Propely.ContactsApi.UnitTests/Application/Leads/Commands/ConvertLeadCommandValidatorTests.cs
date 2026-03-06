// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.ContactsApi.Application.Leads.Commands.ConvertLead;

namespace Propely.ContactsApi.UnitTests.Application.Leads.Commands;

public class ConvertLeadCommandValidatorTests
{
    private readonly ConvertLeadCommandValidator _validator = new();

    private static ConvertLeadCommand ValidCommand() => new()
    {
        LeadId = Guid.NewGuid(),
        TenantId = Guid.NewGuid()
    };

    [Fact]
    public void Validate_ValidCommand_PassesWithNoErrors()
    {
        var result = _validator.Validate(ValidCommand());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyLeadId_FailsValidation()
    {
        var cmd = ValidCommand() with { LeadId = Guid.Empty };
        var result = _validator.Validate(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(cmd.LeadId));
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
