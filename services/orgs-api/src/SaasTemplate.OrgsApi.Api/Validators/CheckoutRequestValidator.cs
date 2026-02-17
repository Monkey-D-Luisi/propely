// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Api.Dtos;
using FluentValidation;

namespace SaasTemplate.OrgsApi.Api.Validators;

public sealed class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
{
    public CheckoutRequestValidator()
    {
        RuleFor(x => x.OrgId)
            .NotEmpty().WithMessage("Organization ID is required.");

        RuleFor(x => x.PlanId)
            .NotEmpty().WithMessage("Plan ID is required.");

        RuleFor(x => x.SuccessUrl)
            .NotEmpty().WithMessage("Success URL is required.");
        RuleFor(x => x.SuccessUrl)
            .MaximumLength(2048).WithMessage("Success URL must not exceed 2048 characters.")
            .Must(UrlValidation.IsAllowedRelativePath)
            .WithMessage("Success URL must be a relative path starting with '/'.")
            .When(x => !string.IsNullOrEmpty(x.SuccessUrl));

        RuleFor(x => x.CancelUrl)
            .NotEmpty().WithMessage("Cancel URL is required.");
        RuleFor(x => x.CancelUrl)
            .MaximumLength(2048).WithMessage("Cancel URL must not exceed 2048 characters.")
            .Must(UrlValidation.IsAllowedRelativePath)
            .WithMessage("Cancel URL must be a relative path starting with '/'.")
            .When(x => !string.IsNullOrEmpty(x.CancelUrl));
    }
}
