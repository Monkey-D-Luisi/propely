// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using Propely.ContactsApi.Application.Common.Interfaces;
using Propely.ContactsApi.Application.Contacts.Commands.CreateContact;
using Propely.ContactsApi.Application.Contacts.Interfaces;
using Propely.ContactsApi.Domain.Contacts;

namespace Propely.ContactsApi.UnitTests.Application.Contacts.Commands;

public class CreateContactCommandHandlerTests
{
    private readonly IContactRepository _contactRepository = Substitute.For<IContactRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateContactCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();

    public CreateContactCommandHandlerTests()
    {
        _handler = new CreateContactCommandHandler(_contactRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesContactAndReturnsDto()
    {
        var command = new CreateContactCommand
        {
            FirstName = "Ana",
            LastName = "Lopez",
            Email = "ana@example.com",
            TenantId = TenantId,
            Roles = [ContactRole.Buyer]
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.FirstName.Should().Be("Ana");
        result.LastName.Should().Be("Lopez");
        result.Email.Should().Be("ana@example.com");
        result.Roles.Should().Contain(ContactRole.Buyer);
        result.TenantId.Should().Be(TenantId);

        await _contactRepository.Received(1).AddAsync(Arg.Any<Contact>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithOptionalFields_MapsAllOptionalFieldsToDto()
    {
        var agentId = Guid.NewGuid();
        var command = new CreateContactCommand
        {
            FirstName = "Pedro",
            LastName = "Gómez",
            Email = "pedro@example.com",
            TenantId = TenantId,
            Roles = [ContactRole.Seller],
            Phone = "+34 600 000 000",
            Company = "Inmobiliaria S.L.",
            Notes = "Important client",
            AssignedAgentId = agentId
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Phone.Should().Be("+34 600 000 000");
        result.Company.Should().Be("Inmobiliaria S.L.");
        result.Notes.Should().Be("Important client");
        result.AssignedAgentId.Should().Be(agentId);
    }
}
