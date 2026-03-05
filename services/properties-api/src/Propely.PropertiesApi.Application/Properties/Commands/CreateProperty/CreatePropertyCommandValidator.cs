// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentValidation;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.Application.Properties.Commands.CreateProperty;

public sealed class CreatePropertyCommandValidator : AbstractValidator<CreatePropertyCommand>
{
    public CreatePropertyCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Property title is required.")
            .MaximumLength(Property.TitleMaxLength)
            .WithMessage($"Property title must not exceed {Property.TitleMaxLength} characters.");

        RuleFor(x => x.PropertyType)
            .IsInEnum().WithMessage("Invalid property type.");

        RuleFor(x => x.OperationType)
            .IsInEnum().WithMessage("Invalid operation type.");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.");

        RuleFor(x => x.AgentId)
            .NotEmpty().WithMessage("Agent ID is required.");

        When(x => x.Financials is not null, () =>
        {
            RuleFor(x => x.Financials!.Price)
                .GreaterThanOrEqualTo(0).When(x => x.Financials!.Price.HasValue)
                .WithMessage("Price cannot be negative.");
        });
    }
}
