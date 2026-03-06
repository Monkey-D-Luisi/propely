// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.ContactsApi.Application.Common.Models;
using Propely.ContactsApi.Application.Contacts.Interfaces;
using Propely.ContactsApi.Application.Contacts.Queries.ListContacts;
using Propely.ContactsApi.Domain.Contacts;

namespace Propely.ContactsApi.UnitTests.Application.Contacts.Queries;

public class ListContactsQueryHandlerTests
{
    private readonly IContactReadRepository _contactReadRepository = Substitute.For<IContactReadRepository>();
    private readonly ListContactsQueryHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();

    public ListContactsQueryHandlerTests()
    {
        _handler = new ListContactsQueryHandler(_contactReadRepository);
    }

    [Fact]
    public async Task Handle_WithContacts_ReturnsMappedPagedResult()
    {
        var contact1 = Contact.Create(
            firstName: "Ana",
            lastName: "Lopez",
            email: "ana@example.com",
            tenantId: TenantId,
            roles: [ContactRole.Buyer]);

        var contact2 = Contact.Create(
            firstName: "Pedro",
            lastName: "Gómez",
            email: "pedro@example.com",
            tenantId: TenantId,
            roles: [ContactRole.Seller]);

        var pagedResult = new PagedResult<Contact>([contact1, contact2], totalCount: 2, pageNumber: 1, pageSize: 20);

        _contactReadRepository.ListAsync(Arg.Any<ContactListFilter>(), Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new ListContactsQuery { TenantId = TenantId, Page = 1, PageSize = 20 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.Items.Should().Contain(i => i.Email == "ana@example.com");
        result.Items.Should().Contain(i => i.Email == "pedro@example.com");
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyPagedResult()
    {
        var pagedResult = new PagedResult<Contact>([], totalCount: 0, pageNumber: 1, pageSize: 20);

        _contactReadRepository.ListAsync(Arg.Any<ContactListFilter>(), Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new ListContactsQuery { TenantId = TenantId, Page = 1, PageSize = 20 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
