// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Api.Dtos;
using Propely.OrgsApi.Api.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Propely.OrgsApi.UnitTests.Api.Validators;

public sealed class PaymentRequestValidatorTests
{
    private readonly PaymentRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_ShouldHaveNoErrors()
    {
        var request = new PaymentRequest(Guid.NewGuid(), 5000, "usd", "Add-on feature", "/en/billing", "/en/billing");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyOrgId_ShouldHaveError()
    {
        var request = new PaymentRequest(Guid.Empty, 5000, "usd", "Test", "/en/billing", "/en/billing");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.OrgId);
    }

    [Fact]
    public void Validate_WithZeroAmount_ShouldHaveError()
    {
        var request = new PaymentRequest(Guid.NewGuid(), 0, "usd", "Test", "/en/billing", "/en/billing");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Validate_WithNegativeAmount_ShouldHaveError()
    {
        var request = new PaymentRequest(Guid.NewGuid(), -100, "usd", "Test", "/en/billing", "/en/billing");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Validate_WithEmptyCurrency_ShouldHaveError()
    {
        var request = new PaymentRequest(Guid.NewGuid(), 5000, "", "Test", "/en/billing", "/en/billing");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Fact]
    public void Validate_WithEmptyDescription_ShouldHaveError()
    {
        var request = new PaymentRequest(Guid.NewGuid(), 5000, "usd", "", "/en/billing", "/en/billing");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_WithAbsoluteSuccessUrl_ShouldHaveError()
    {
        var request = new PaymentRequest(Guid.NewGuid(), 5000, "usd", "Test", "https://evil.com/success", "/en/billing");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.SuccessUrl);
    }

    [Fact]
    public void Validate_WithEmptySuccessUrl_ShouldHaveError()
    {
        var request = new PaymentRequest(Guid.NewGuid(), 5000, "usd", "Test", "", "/en/billing");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.SuccessUrl);
    }

    [Fact]
    public void Validate_WithEmptyCancelUrl_ShouldHaveError()
    {
        var request = new PaymentRequest(Guid.NewGuid(), 5000, "usd", "Test", "/en/billing", "");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.CancelUrl);
    }

    [Fact]
    public void Validate_WithAbsoluteCancelUrl_ShouldHaveError()
    {
        var request = new PaymentRequest(Guid.NewGuid(), 5000, "usd", "Test", "/en/billing", "https://evil.com/cancel");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.CancelUrl);
    }

    [Fact]
    public void Validate_WithProtocolRelativeCancelUrl_ShouldHaveError()
    {
        var request = new PaymentRequest(Guid.NewGuid(), 5000, "usd", "Test", "/en/billing", "//evil.com/cancel");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.CancelUrl);
    }
}
