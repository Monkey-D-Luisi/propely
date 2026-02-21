// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Agencies;
using FluentValidation;

namespace Propely.OrgsApi.Application.Agencies.Commands.CreateAgency;

public sealed class CreateAgencyCommandValidator : AbstractValidator<CreateAgencyCommand>
{
    public CreateAgencyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Agency name is required.")
            .MaximumLength(Agency.NameMaxLength)
            .WithMessage($"Agency name must not exceed {Agency.NameMaxLength} characters.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Agency slug is required.")
            .MinimumLength(AgencySlug.MinLength)
            .WithMessage($"Agency slug must be at least {AgencySlug.MinLength} characters.")
            .MaximumLength(AgencySlug.MaxLength)
            .WithMessage($"Agency slug must not exceed {AgencySlug.MaxLength} characters.")
            .Matches(@"^[a-z0-9](?:[a-z0-9]|-(?!-))*[a-z0-9]$")
            .WithMessage("Agency slug must contain only lowercase letters, numbers, and hyphens, and must start and end with a letter or number.");

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("Creator user ID is required.");
    }
}
