// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Api.Dtos;
using Propely.OrgsApi.Domain.Organizations;
using FluentValidation;

namespace Propely.OrgsApi.Api.Validators;

public sealed class InviteRequestValidator : AbstractValidator<InviteRequest>
{
    private static readonly string[] ValidRoles = Enum.GetNames<MembershipRole>()
        .Where(r => r != nameof(MembershipRole.Owner))
        .Select(r => r.ToLowerInvariant())
        .ToArray();

    public InviteRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(r => ValidRoles.Contains(r.ToLowerInvariant()))
            .WithMessage($"Role must be one of: {string.Join(", ", ValidRoles)}.");
    }
}
