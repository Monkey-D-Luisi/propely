// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Api.Dtos;
using FluentValidation;

namespace Propely.OrgsApi.Api.Validators;

public sealed class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    private static readonly string[] ValidRoles = ["owner", "admin", "member", "viewer"];

    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(r => ValidRoles.Contains(r.ToLowerInvariant()))
            .WithMessage("Role must be one of: owner, admin, member, viewer.");
    }
}
