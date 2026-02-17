// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Api.Dtos;
using SaasTemplate.OrgsApi.Domain.Organizations;
using FluentValidation;

namespace SaasTemplate.OrgsApi.Api.Validators;

public sealed class CreateOrgRequestValidator : AbstractValidator<CreateOrgRequest>
{
    public CreateOrgRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Name is required.")
            .MaximumLength(Organization.NameMaxLength)
            .WithMessage($"Name must not exceed {Organization.NameMaxLength} characters.");
    }
}
