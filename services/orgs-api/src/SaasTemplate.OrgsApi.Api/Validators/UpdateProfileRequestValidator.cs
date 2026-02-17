// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Api.Dtos;
using SaasTemplate.OrgsApi.Domain.Users;
using FluentValidation;

namespace SaasTemplate.OrgsApi.Api.Validators;

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
