// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Propely.ContactsApi.Application.Contacts.Interfaces;
using Propely.ContactsApi.Application.Contacts.Queries.GetContactById;
using Propely.ContactsApi.Domain.Contacts;

namespace Propely.ContactsApi.UnitTests.Application.Contacts.Queries;

public class GetContactByIdQueryHandlerTests
{
    private readonly IContactReadRepository _contactReadRepository = Substitute.For<IContactReadRepository>();
    private readonly GetContactByIdQueryHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();

    public GetContactByIdQueryHandlerTests()
    {
        _handler = new GetContactByIdQueryHandler(_contactReadRepository);
    }

    [Fact]
    public async Task Handle_ContactExists_ReturnsDto()
    {
        var contact = Contact.Create(
            firstName: "Ana",
            lastName: "Lopez",
            email: "ana@example.com",
            tenantId: TenantId,
            roles: [ContactRole.Buyer]);

        var query = new GetContactByIdQuery(contact.Id, TenantId);

        _contactReadRepository.GetByIdAsync(query.ContactId, TenantId, Arg.Any<CancellationToken>())
            .Returns(contact);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(contact.Id);
        result.FirstName.Should().Be("Ana");
        result.Email.Should().Be("ana@example.com");
    }

    [Fact]
    public async Task Handle_ContactNotFound_ReturnsNull()
    {
        var query = new GetContactByIdQuery(Guid.NewGuid(), TenantId);

        _contactReadRepository.GetByIdAsync(query.ContactId, TenantId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}
