// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentValidation;

namespace Propely.AiApi.Application.Actions.Commands.ExecuteAction;

/// <summary>
/// Validates the ExecuteActionCommand ensuring text is present and within limits.
/// </summary>
public sealed class ExecuteActionCommandValidator : AbstractValidator<ExecuteActionCommand>
{
    public ExecuteActionCommandValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage("Text is required.")
            .MaximumLength(2000)
            .WithMessage("Text cannot exceed 2000 characters.");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.AgentId)
            .NotEmpty()
            .WithMessage("Agent ID is required.");
    }
}
