// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Api.Dtos;
using Propely.OrgsApi.Domain.Organizations;
using FluentValidation;

namespace Propely.OrgsApi.Api.Validators;

public sealed class UpdateOrgRequestValidator : AbstractValidator<UpdateOrgRequest>
{
    public UpdateOrgRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Name is required.")
            .MaximumLength(Organization.NameMaxLength)
            .WithMessage($"Name must not exceed {Organization.NameMaxLength} characters.");

        RuleFor(x => x.Description)
            .MaximumLength(Organization.DescriptionMaxLength)
            .WithMessage($"Description must not exceed {Organization.DescriptionMaxLength} characters.")
            .When(x => x.Description is not null);
    }
}
