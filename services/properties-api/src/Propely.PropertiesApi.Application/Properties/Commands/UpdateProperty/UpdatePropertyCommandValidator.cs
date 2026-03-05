// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentValidation;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.Application.Properties.Commands.UpdateProperty;

public sealed class UpdatePropertyCommandValidator : AbstractValidator<UpdatePropertyCommand>
{
    public UpdatePropertyCommandValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty().WithMessage("Property ID is required.");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        When(x => x.Title is not null, () =>
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Property title cannot be empty.")
                .MaximumLength(Property.TitleMaxLength)
                .WithMessage($"Property title must not exceed {Property.TitleMaxLength} characters.");
        });

        When(x => x.PropertyType.HasValue, () =>
        {
            RuleFor(x => x.PropertyType!.Value)
                .IsInEnum().WithMessage("Invalid property type.");
        });

        When(x => x.OperationType.HasValue, () =>
        {
            RuleFor(x => x.OperationType!.Value)
                .IsInEnum().WithMessage("Invalid operation type.");
        });

        When(x => x.Financials is not null, () =>
        {
            RuleFor(x => x.Financials!.Price)
                .GreaterThanOrEqualTo(0).When(x => x.Financials!.Price.HasValue)
                .WithMessage("Price cannot be negative.");
        });
    }
}
