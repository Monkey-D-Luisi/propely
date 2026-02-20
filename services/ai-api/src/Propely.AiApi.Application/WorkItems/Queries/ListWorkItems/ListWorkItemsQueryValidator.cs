// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentValidation;

namespace Propely.AiApi.Application.WorkItems.Queries.ListWorkItems;

public class ListWorkItemsQueryValidator : AbstractValidator<ListWorkItemsQuery>
{
    public ListWorkItemsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("PageSize at least greater than or equal to 1.")
            .LessThanOrEqualTo(50).WithMessage("PageSize must not exceed 50.");
    }
}
