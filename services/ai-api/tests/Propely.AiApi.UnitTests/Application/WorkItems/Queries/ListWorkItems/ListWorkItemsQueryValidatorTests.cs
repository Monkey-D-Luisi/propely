// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Application.WorkItems.Queries.ListWorkItems;
using FluentValidation.TestHelper;
using Xunit;

namespace Propely.AiApi.UnitTests.Application.WorkItems.Queries.ListWorkItems;

public class ListWorkItemsQueryValidatorTests
{
    private readonly ListWorkItemsQueryValidator _validator = new();

    [Fact]
    public void Validate_ShouldHaveError_WhenPageIsLessThanOne()
    {
        var query = new ListWorkItemsQuery(Page: 0);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPageSizeIsLessThanOne()
    {
        var query = new ListWorkItemsQuery(PageSize: 0);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPageSizeIsGreaterThan50()
    {
        var query = new ListWorkItemsQuery(PageSize: 51);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenValuesAreValid()
    {
        var query = new ListWorkItemsQuery(Page: 1, PageSize: 10);
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
