// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.AppointmentsApi.Application.Common.Models;

namespace Propely.AppointmentsApi.UnitTests.Application.Common.Models;

public sealed class PagedResultTests
{
    [Fact]
    public void Constructor_Default_ShouldInitializeEmpty()
    {
        var result = new PagedResult<string>();

        result.Items.Should().BeEmpty();
        result.PageNumber.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithItems_ShouldCalculateTotalPages()
    {
        var items = new[] { "a", "b", "c" };

        var result = new PagedResult<string>(items, totalCount: 10, pageNumber: 1, pageSize: 3);

        result.Items.Should().BeEquivalentTo(items);
        result.TotalCount.Should().Be(10);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().Be(4); // ceil(10/3) = 4
    }

    [Fact]
    public void HasPreviousPage_WhenFirstPage_ShouldBeFalse()
    {
        var result = new PagedResult<string>([], totalCount: 10, pageNumber: 1, pageSize: 5);

        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_WhenSecondPage_ShouldBeTrue()
    {
        var result = new PagedResult<string>([], totalCount: 10, pageNumber: 2, pageSize: 5);

        result.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_WhenLastPage_ShouldBeFalse()
    {
        var result = new PagedResult<string>([], totalCount: 10, pageNumber: 2, pageSize: 5);

        result.HasNextPage.Should().BeFalse(); // 2 pages total, on page 2
    }

    [Fact]
    public void HasNextPage_WhenNotLastPage_ShouldBeTrue()
    {
        var result = new PagedResult<string>([], totalCount: 10, pageNumber: 1, pageSize: 5);

        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void TotalPages_WhenExactDivision_ShouldNotRoundUp()
    {
        var result = new PagedResult<string>([], totalCount: 10, pageNumber: 1, pageSize: 5);

        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public void TotalPages_WhenZeroItems_ShouldBeZero()
    {
        var result = new PagedResult<string>([], totalCount: 0, pageNumber: 1, pageSize: 10);

        result.TotalPages.Should().Be(0);
    }
}
