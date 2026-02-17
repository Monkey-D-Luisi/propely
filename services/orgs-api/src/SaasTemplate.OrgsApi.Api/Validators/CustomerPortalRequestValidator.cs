// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Api.Dtos;
using FluentValidation;

namespace SaasTemplate.OrgsApi.Api.Validators;

public sealed class CustomerPortalRequestValidator : AbstractValidator<CustomerPortalRequest>
{
    public CustomerPortalRequestValidator()
    {
        RuleFor(x => x.OrgId)
            .NotEmpty().WithMessage("Organization ID is required.");

        RuleFor(x => x.ReturnUrl)
            .NotEmpty().WithMessage("Return URL is required.");
        RuleFor(x => x.ReturnUrl)
            .MaximumLength(2048).WithMessage("Return URL must not exceed 2048 characters.")
            .Must(UrlValidation.IsAllowedRelativePath)
            .WithMessage("Return URL must be a relative path starting with '/'.")
            .When(x => !string.IsNullOrEmpty(x.ReturnUrl));
    }
}
