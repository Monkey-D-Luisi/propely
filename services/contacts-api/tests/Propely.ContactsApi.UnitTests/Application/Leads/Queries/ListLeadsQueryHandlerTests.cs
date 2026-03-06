// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.ContactsApi.Application.Common.Models;
using Propely.ContactsApi.Application.Leads.Interfaces;
using Propely.ContactsApi.Application.Leads.Queries.ListLeads;
using Propely.ContactsApi.Domain.Leads;

namespace Propely.ContactsApi.UnitTests.Application.Leads.Queries;

public class ListLeadsQueryHandlerTests
{
    private readonly ILeadReadRepository _leadReadRepository = Substitute.For<ILeadReadRepository>();
    private readonly ListLeadsQueryHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();

    public ListLeadsQueryHandlerTests()
    {
        _handler = new ListLeadsQueryHandler(_leadReadRepository);
    }

    [Fact]
    public async Task Handle_WithLeads_ReturnsMappedPagedResult()
    {
        var propertyId = Guid.NewGuid();
        var lead1 = Lead.Create(
            name: "Maria Garcia",
            email: "maria@example.com",
            propertyId: propertyId,
            tenantId: TenantId);

        var lead2 = Lead.Create(
            name: "Carlos Ruiz",
            email: "carlos@example.com",
            propertyId: propertyId,
            tenantId: TenantId);

        var pagedResult = new PagedResult<Lead>([lead1, lead2], totalCount: 2, pageNumber: 1, pageSize: 20);

        _leadReadRepository.ListAsync(Arg.Any<LeadListFilter>(), Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new ListLeadsQuery { TenantId = TenantId, Page = 1, PageSize = 20 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.Items.Should().Contain(l => l.Email == "maria@example.com");
        result.Items.Should().Contain(l => l.Email == "carlos@example.com");
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyPagedResult()
    {
        var pagedResult = new PagedResult<Lead>([], totalCount: 0, pageNumber: 1, pageSize: 20);

        _leadReadRepository.ListAsync(Arg.Any<LeadListFilter>(), Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new ListLeadsQuery { TenantId = TenantId, Page = 1, PageSize = 20 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
