// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Api.Dtos;
using Propely.OrgsApi.Domain.Agencies;
using FluentValidation;

namespace Propely.OrgsApi.Api.Validators;

public sealed class CreateAgencyRequestValidator : AbstractValidator<CreateAgencyRequest>
{
    public CreateAgencyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(Agency.NameMaxLength)
            .WithMessage($"Name must not exceed {Agency.NameMaxLength} characters.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MinimumLength(AgencySlug.MinLength)
            .WithMessage($"Slug must be at least {AgencySlug.MinLength} characters.")
            .MaximumLength(AgencySlug.MaxLength)
            .WithMessage($"Slug must not exceed {AgencySlug.MaxLength} characters.")
            .Matches(@"^[a-z0-9](?:[a-z0-9]|-(?!-))*[a-z0-9]$")
            .WithMessage("Slug must contain only lowercase letters, numbers, and hyphens, and must start and end with a letter or number.");
    }
}
