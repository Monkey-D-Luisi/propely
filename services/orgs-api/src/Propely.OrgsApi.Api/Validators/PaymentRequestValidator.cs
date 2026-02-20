// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Api.Dtos;
using FluentValidation;

namespace Propely.OrgsApi.Api.Validators;

public sealed class PaymentRequestValidator : AbstractValidator<PaymentRequest>
{
    public PaymentRequestValidator()
    {
        RuleFor(x => x.OrgId)
            .NotEmpty().WithMessage("Organization ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(10).WithMessage("Currency must not exceed 10 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.SuccessUrl)
            .NotEmpty().WithMessage("Success URL is required.")
            .MaximumLength(2048).WithMessage("Success URL must not exceed 2048 characters.")
            .Must(UrlValidation.IsAllowedRelativePath)
            .WithMessage("Success URL must be a relative path starting with '/'.");

        RuleFor(x => x.CancelUrl)
            .NotEmpty().WithMessage("Cancel URL is required.")
            .MaximumLength(2048).WithMessage("Cancel URL must not exceed 2048 characters.")
            .Must(UrlValidation.IsAllowedRelativePath)
            .WithMessage("Cancel URL must be a relative path starting with '/'.");
    }
}
