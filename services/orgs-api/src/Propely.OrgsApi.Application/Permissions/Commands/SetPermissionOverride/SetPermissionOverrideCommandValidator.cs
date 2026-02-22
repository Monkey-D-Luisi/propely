// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentValidation;
using Propely.OrgsApi.Domain.Permissions;

namespace Propely.OrgsApi.Application.Permissions.Commands.SetPermissionOverride;

public sealed class SetPermissionOverrideCommandValidator : AbstractValidator<SetPermissionOverrideCommand>
{
    public SetPermissionOverrideCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage("Organization ID is required.");

        RuleFor(x => x.TargetUserId)
            .NotEmpty().WithMessage("Target user ID is required.");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("Requesting user ID is required.");

        RuleFor(x => x.Permission)
            .IsInEnum().WithMessage("Invalid permission value.");
    }
}
