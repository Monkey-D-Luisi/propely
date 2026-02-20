// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Api.Dtos;
using Propely.OrgsApi.Api.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Propely.OrgsApi.UnitTests.Api.Validators;

public sealed class CheckoutRequestValidatorTests
{
    private readonly CheckoutRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_ShouldHaveNoErrors()
    {
        var request = new CheckoutRequest(Guid.NewGuid(), "pro", "/en/billing", "/en/pricing");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyOrgId_ShouldHaveError()
    {
        var request = new CheckoutRequest(Guid.Empty, "pro", "/en/billing", "/en/pricing");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.OrgId);
    }

    [Fact]
    public void Validate_WithEmptyPlanId_ShouldHaveError()
    {
        var request = new CheckoutRequest(Guid.NewGuid(), "", "/en/billing", "/en/pricing");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PlanId);
    }

    [Fact]
    public void Validate_WithEmptySuccessUrl_ShouldHaveError()
    {
        var request = new CheckoutRequest(Guid.NewGuid(), "pro", "", "/en/pricing");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.SuccessUrl);
    }

    [Fact]
    public void Validate_WithEmptyCancelUrl_ShouldHaveError()
    {
        var request = new CheckoutRequest(Guid.NewGuid(), "pro", "/en/billing", "");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.CancelUrl);
    }

    [Fact]
    public void Validate_WithAbsoluteSuccessUrl_ShouldHaveError()
    {
        var request = new CheckoutRequest(Guid.NewGuid(), "pro", "https://evil.com", "/en/pricing");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.SuccessUrl);
    }

    [Fact]
    public void Validate_WithProtocolRelativeSuccessUrl_ShouldHaveError()
    {
        var request = new CheckoutRequest(Guid.NewGuid(), "pro", "//evil.com", "/en/pricing");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.SuccessUrl);
    }

    [Fact]
    public void Validate_WithOverlongSuccessUrl_ShouldHaveError()
    {
        var longUrl = "/" + new string('a', 2048);
        var request = new CheckoutRequest(Guid.NewGuid(), "pro", longUrl, "/en/pricing");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.SuccessUrl);
    }

    [Fact]
    public void Validate_WithUrlAtMaxLength_ShouldHaveNoError()
    {
        var maxUrl = "/" + new string('a', 2046);
        var request = new CheckoutRequest(Guid.NewGuid(), "pro", maxUrl, "/en/pricing");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.SuccessUrl);
    }
}
