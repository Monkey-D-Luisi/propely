// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentValidation;

namespace Propely.OrgsApi.Application.Users.Commands.OAuthLogin;

public sealed class OAuthLoginCommandValidator : AbstractValidator<OAuthLoginCommand>
{
    public OAuthLoginCommandValidator()
    {
        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required.");

        RuleFor(x => x.ExternalId)
            .NotEmpty().WithMessage("External ID is required.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}
