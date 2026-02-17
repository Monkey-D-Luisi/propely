// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.AiApi.Api.Dtos;
using SaasTemplate.AiApi.Domain.WorkItems;
using FluentValidation;

namespace SaasTemplate.AiApi.Api.Validators;

public sealed class UpdateWorkItemRequestValidator : AbstractValidator<UpdateWorkItemRequest>
{
    public UpdateWorkItemRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(WorkItem.TitleMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(WorkItem.DescriptionMaxLength);

        RuleFor(x => x.Status)
            .IsInEnum();

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
