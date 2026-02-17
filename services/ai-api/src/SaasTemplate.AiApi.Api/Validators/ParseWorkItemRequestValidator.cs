// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentValidation;
using SaasTemplate.AiApi.Api.Dtos;

namespace SaasTemplate.AiApi.Api.Validators;

/// <summary>
/// Validator for ParseWorkItemRequest.
/// </summary>
public class ParseWorkItemRequestValidator : AbstractValidator<ParseWorkItemRequest>
{
    public ParseWorkItemRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage("Text is required.")
            .MaximumLength(2000)
            .WithMessage("Text cannot exceed 2000 characters.");
    }
}
