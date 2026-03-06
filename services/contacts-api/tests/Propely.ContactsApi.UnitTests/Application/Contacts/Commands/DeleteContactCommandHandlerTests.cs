// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Propely.ContactsApi.Application.Common.Interfaces;
using Propely.ContactsApi.Application.Contacts.Commands.DeleteContact;
using Propely.ContactsApi.Application.Contacts.Interfaces;
using Propely.ContactsApi.Domain.Common.Exceptions;
using Propely.ContactsApi.Domain.Contacts;

namespace Propely.ContactsApi.UnitTests.Application.Contacts.Commands;

public class DeleteContactCommandHandlerTests
{
    private readonly IContactRepository _contactRepository = Substitute.For<IContactRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteContactCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();

    public DeleteContactCommandHandlerTests()
    {
        _handler = new DeleteContactCommandHandler(_contactRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ContactFound_SoftDeletesAndSaves()
    {
        var contact = Contact.Create(
            firstName: "Ana",
            lastName: "Lopez",
            email: "ana@example.com",
            tenantId: TenantId,
            roles: [ContactRole.Buyer]);

        var command = new DeleteContactCommand(contact.Id, TenantId);

        _contactRepository.GetByIdAsync(command.ContactId, TenantId, Arg.Any<CancellationToken>())
            .Returns(contact);

        await _handler.Handle(command, CancellationToken.None);

        contact.IsDeleted.Should().BeTrue();
        _contactRepository.Received(1).Update(contact);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ContactNotFound_ThrowsNotFoundException()
    {
        var command = new DeleteContactCommand(Guid.NewGuid(), TenantId);

        _contactRepository.GetByIdAsync(command.ContactId, TenantId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
