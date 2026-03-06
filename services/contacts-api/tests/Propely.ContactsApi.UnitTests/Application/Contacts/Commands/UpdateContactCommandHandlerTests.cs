// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Propely.ContactsApi.Application.Common.Interfaces;
using Propely.ContactsApi.Application.Contacts.Commands.UpdateContact;
using Propely.ContactsApi.Application.Contacts.Interfaces;
using Propely.ContactsApi.Domain.Common.Exceptions;
using Propely.ContactsApi.Domain.Contacts;

namespace Propely.ContactsApi.UnitTests.Application.Contacts.Commands;

public class UpdateContactCommandHandlerTests
{
    private readonly IContactRepository _contactRepository = Substitute.For<IContactRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateContactCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();

    public UpdateContactCommandHandlerTests()
    {
        _handler = new UpdateContactCommandHandler(_contactRepository, _unitOfWork);
    }

    private static Contact CreateContact() => Contact.Create(
        firstName: "Ana",
        lastName: "Lopez",
        email: "ana@example.com",
        tenantId: TenantId,
        roles: [ContactRole.Buyer]);

    [Fact]
    public async Task Handle_ContactFound_UpdatesAndReturnsDto()
    {
        var contact = CreateContact();
        var command = new UpdateContactCommand
        {
            ContactId = contact.Id,
            TenantId = TenantId,
            FirstName = "Anna",
            LastName = "Lopez",
            Email = "anna@example.com",
            Roles = [ContactRole.Seller]
        };

        _contactRepository.GetByIdAsync(command.ContactId, TenantId, Arg.Any<CancellationToken>())
            .Returns(contact);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.FirstName.Should().Be("Anna");
        _contactRepository.Received(1).Update(contact);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ContactNotFound_ThrowsNotFoundException()
    {
        var command = new UpdateContactCommand
        {
            ContactId = Guid.NewGuid(),
            TenantId = TenantId,
            FirstName = "X"
        };

        _contactRepository.GetByIdAsync(command.ContactId, TenantId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
