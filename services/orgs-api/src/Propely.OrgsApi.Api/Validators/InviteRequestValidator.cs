// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Api.Dtos;
using FluentValidation;

namespace Propely.OrgsApi.Api.Validators;

public sealed class InviteRequestValidator : AbstractValidator<InviteRequest>
{
    private static readonly string[] ValidRoles = ["admin", "member", "viewer"];

    public InviteRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(r => ValidRoles.Contains(r.ToLowerInvariant()))
            .WithMessage("Role must be one of: admin, member, viewer.");
    }
}
