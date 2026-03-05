// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentValidation;
using Propely.AiApi.Api.Dtos;

namespace Propely.AiApi.Api.Validators;

/// <summary>
/// Validator for ExecuteActionRequest ensuring text is present and within limits.
/// </summary>
public sealed class ExecuteActionRequestValidator : AbstractValidator<ExecuteActionRequest>
{
    public ExecuteActionRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage("Text is required.")
            .MaximumLength(2000)
            .WithMessage("Text cannot exceed 2000 characters.");
    }
}
