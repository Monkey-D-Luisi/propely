// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentValidation;

namespace Propely.PropertiesApi.Application.Properties.Commands.ChangeStatus;

public sealed class ChangePropertyStatusCommandValidator : AbstractValidator<ChangePropertyStatusCommand>
{
    public ChangePropertyStatusCommandValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty().WithMessage("Property ID is required.");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid property status.");
    }
}
