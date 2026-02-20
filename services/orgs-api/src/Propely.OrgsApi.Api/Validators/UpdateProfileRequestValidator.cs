// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Api.Dtos;
using Propely.OrgsApi.Domain.Users;
using FluentValidation;

namespace Propely.OrgsApi.Api.Validators;

public sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(User.NameMaxLength)
            .WithMessage($"Name must not exceed {User.NameMaxLength} characters.")
            .When(x => x.Name is not null);
    }
}
