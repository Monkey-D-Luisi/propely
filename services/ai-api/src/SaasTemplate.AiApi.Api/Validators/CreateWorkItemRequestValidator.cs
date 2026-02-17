// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.AiApi.Api.Dtos;
using SaasTemplate.AiApi.Domain.WorkItems;
using FluentValidation;

namespace SaasTemplate.AiApi.Api.Validators;

/// <summary>
/// Validator for CreateWorkItemRequest.
/// </summary>
public sealed class CreateWorkItemRequestValidator : AbstractValidator<CreateWorkItemRequest>
{
    /// <summary>
    /// Initializes validation rules.
    /// </summary>
    public CreateWorkItemRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .Must(s => !string.IsNullOrWhiteSpace(s))
            .WithMessage("Title cannot be whitespace only.")
            .MaximumLength(WorkItem.TitleMaxLength)
            .WithMessage($"Title must not exceed {WorkItem.TitleMaxLength} characters.");

        RuleFor(x => x.Description)
            .MaximumLength(WorkItem.DescriptionMaxLength)
            .WithMessage($"Description must not exceed {WorkItem.DescriptionMaxLength} characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.Priority)
            .Must(v => Enum.TryParse<WorkItemPriority>(v, out _))
            .WithMessage("Priority must be one of: Low, Medium, High, Critical.")
            .When(x => x.Priority is not null);

        RuleFor(x => x.Type)
            .Must(v => Enum.TryParse<WorkItemType>(v, out _))
            .WithMessage("Type must be one of: Task, Bug, Feature, Improvement.")
            .When(x => x.Type is not null);

        RuleFor(x => x.EstimatedEffort)
            .Must(v => Enum.TryParse<WorkItemEffort>(v, out _))
            .WithMessage("EstimatedEffort must be one of: XS, S, M, L, XL.")
            .When(x => x.EstimatedEffort is not null);
    }
}
