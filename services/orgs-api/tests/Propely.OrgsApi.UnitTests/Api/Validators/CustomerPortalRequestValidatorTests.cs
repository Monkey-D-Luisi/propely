// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Api.Dtos;
using Propely.OrgsApi.Api.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Propely.OrgsApi.UnitTests.Api.Validators;

public sealed class CustomerPortalRequestValidatorTests
{
    private readonly CustomerPortalRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_ShouldHaveNoErrors()
    {
        var request = new CustomerPortalRequest(Guid.NewGuid(), "/en/billing");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyOrgId_ShouldHaveError()
    {
        var request = new CustomerPortalRequest(Guid.Empty, "/en/billing");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.OrgId);
    }

    [Fact]
    public void Validate_WithEmptyReturnUrl_ShouldHaveError()
    {
        var request = new CustomerPortalRequest(Guid.NewGuid(), "");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ReturnUrl);
    }

    [Fact]
    public void Validate_WithAbsoluteReturnUrl_ShouldHaveError()
    {
        var request = new CustomerPortalRequest(Guid.NewGuid(), "https://evil.com");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ReturnUrl);
    }

    [Fact]
    public void Validate_WithProtocolRelativeReturnUrl_ShouldHaveError()
    {
        var request = new CustomerPortalRequest(Guid.NewGuid(), "//evil.com");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ReturnUrl);
    }

    [Fact]
    public void Validate_WithOverlongReturnUrl_ShouldHaveError()
    {
        var longUrl = "/" + new string('a', 2048);
        var request = new CustomerPortalRequest(Guid.NewGuid(), longUrl);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ReturnUrl);
    }
}
