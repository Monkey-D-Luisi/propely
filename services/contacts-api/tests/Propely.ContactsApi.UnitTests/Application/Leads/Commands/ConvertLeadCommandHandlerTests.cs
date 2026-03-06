// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Propely.ContactsApi.Application.Common.Interfaces;
using Propely.ContactsApi.Application.Contacts.Interfaces;
using Propely.ContactsApi.Application.Leads.Commands.ConvertLead;
using Propely.ContactsApi.Application.Leads.Interfaces;
using Propely.ContactsApi.Domain.Common.Exceptions;
using Propely.ContactsApi.Domain.Contacts;
using Propely.ContactsApi.Domain.Leads;

namespace Propely.ContactsApi.UnitTests.Application.Leads.Commands;

public class ConvertLeadCommandHandlerTests
{
    private readonly ILeadRepository _leadRepository = Substitute.For<ILeadRepository>();
    private readonly IContactRepository _contactRepository = Substitute.For<IContactRepository>();
    private readonly IContactReadRepository _contactReadRepository = Substitute.For<IContactReadRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ConvertLeadCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid PropertyId = Guid.NewGuid();

    public ConvertLeadCommandHandlerTests()
    {
        _handler = new ConvertLeadCommandHandler(
            _leadRepository,
            _contactRepository,
            _contactReadRepository,
            _unitOfWork);
    }

    private static Lead CreateQualifiedLead(
        string name = "Maria Garcia",
        string email = "maria@example.com",
        Guid? propertyId = null,
        Guid? tenantId = null)
    {
        var lead = Lead.Create(
            name: name,
            email: email,
            propertyId: propertyId ?? PropertyId,
            tenantId: tenantId ?? TenantId,
            phone: "+34 650 123 456",
            source: "Idealista");

        // Transition: New -> Contacted -> Qualified
        lead.ChangeStatus(LeadStatus.Contacted);
        lead.ChangeStatus(LeadStatus.Qualified);
        lead.ClearDomainEvents();

        return lead;
    }

    [Fact]
    public async Task Handle_LeadNotFound_ThrowsNotFoundException()
    {
        var command = new ConvertLeadCommand
        {
            LeadId = Guid.NewGuid(),
            TenantId = TenantId,
            Role = ContactRole.Buyer
        };

        _leadRepository.GetByIdAsync(command.LeadId, command.TenantId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_LeadNotQualified_ThrowsDomainException()
    {
        var lead = Lead.Create(
            name: "Test User",
            email: "test@example.com",
            propertyId: PropertyId,
            tenantId: TenantId);
        // Lead is in New status -- not qualified

        var command = new ConvertLeadCommand
        {
            LeadId = lead.Id,
            TenantId = TenantId,
            Role = ContactRole.Buyer
        };

        _leadRepository.GetByIdAsync(command.LeadId, command.TenantId, Arg.Any<CancellationToken>())
            .Returns(lead);
        _contactReadRepository.GetByEmailAsync(lead.Email, TenantId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Qualified*");
    }

    [Fact]
    public async Task Handle_NoExistingContact_CreatesNewContact()
    {
        var lead = CreateQualifiedLead();

        var command = new ConvertLeadCommand
        {
            LeadId = lead.Id,
            TenantId = TenantId,
            Role = ContactRole.Buyer,
            Notes = "Conversion notes"
        };

        _leadRepository.GetByIdAsync(command.LeadId, command.TenantId, Arg.Any<CancellationToken>())
            .Returns(lead);
        _contactReadRepository.GetByEmailAsync(lead.Email, TenantId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.WasNewContact.Should().BeTrue();
        result.Contact.Should().NotBeNull();
        result.Contact.Email.Should().Be(lead.Email);
        result.Contact.FirstName.Should().Be("Maria");
        result.Contact.LastName.Should().Be("Garcia");
        result.Contact.Roles.Should().Contain(ContactRole.Buyer);
        result.Lead.Status.Should().Be(LeadStatus.Converted);
        result.Lead.ContactId.Should().Be(result.Contact.Id);

        await _contactRepository.Received(1).AddAsync(Arg.Any<Contact>(), Arg.Any<CancellationToken>());
        _leadRepository.Received(1).Update(lead);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingContact_AddsPropertyInterest()
    {
        var lead = CreateQualifiedLead();

        var existingContact = Contact.Create(
            firstName: "Maria",
            lastName: "Garcia",
            email: lead.Email,
            tenantId: TenantId,
            roles: [ContactRole.Buyer],
            phone: "+34 999 999 999");

        var command = new ConvertLeadCommand
        {
            LeadId = lead.Id,
            TenantId = TenantId,
            Role = ContactRole.Buyer,
            Notes = "Additional interest"
        };

        _leadRepository.GetByIdAsync(command.LeadId, command.TenantId, Arg.Any<CancellationToken>())
            .Returns(lead);
        _contactReadRepository.GetByEmailAsync(lead.Email, TenantId, Arg.Any<CancellationToken>())
            .Returns(existingContact);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.WasNewContact.Should().BeFalse();
        result.Contact.Id.Should().Be(existingContact.Id);
        result.Contact.PropertyInterests.Should().ContainSingle(pi =>
            pi.PropertyId == lead.PropertyId && pi.InterestType == InterestType.Buying);
        result.Lead.Status.Should().Be(LeadStatus.Converted);

        await _contactRepository.DidNotReceive().AddAsync(Arg.Any<Contact>(), Arg.Any<CancellationToken>());
        _contactRepository.Received(1).Update(existingContact);
        _leadRepository.Received(1).Update(lead);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TenantRole_MapsToRentingInterestType()
    {
        var lead = CreateQualifiedLead();

        var command = new ConvertLeadCommand
        {
            LeadId = lead.Id,
            TenantId = TenantId,
            Role = ContactRole.Tenant
        };

        _leadRepository.GetByIdAsync(command.LeadId, command.TenantId, Arg.Any<CancellationToken>())
            .Returns(lead);
        _contactReadRepository.GetByEmailAsync(lead.Email, TenantId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.WasNewContact.Should().BeTrue();
        result.Contact.PropertyInterests.Should().ContainSingle(pi =>
            pi.PropertyId == PropertyId && pi.InterestType == InterestType.Renting);
        result.Contact.Roles.Should().Contain(ContactRole.Tenant);
    }

    [Fact]
    public async Task Handle_AlreadyConverted_ThrowsDomainException()
    {
        var lead = CreateQualifiedLead();
        // Convert the lead first
        lead.Convert(Guid.NewGuid(), true);

        var command = new ConvertLeadCommand
        {
            LeadId = lead.Id,
            TenantId = TenantId,
            Role = ContactRole.Buyer
        };

        _leadRepository.GetByIdAsync(command.LeadId, command.TenantId, Arg.Any<CancellationToken>())
            .Returns(lead);
        _contactReadRepository.GetByEmailAsync(lead.Email, TenantId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*already converted*");
    }

    [Fact]
    public async Task Handle_ExistingContactMissingPhone_UpdatesPhoneFromLead()
    {
        var lead = CreateQualifiedLead();

        var existingContact = Contact.Create(
            firstName: "Maria",
            lastName: "Garcia",
            email: lead.Email,
            tenantId: TenantId,
            roles: [ContactRole.Buyer]);
        // Note: no phone set on contact; lead has phone "+34 650 123 456"

        var command = new ConvertLeadCommand
        {
            LeadId = lead.Id,
            TenantId = TenantId,
            Role = ContactRole.Buyer
        };

        _leadRepository.GetByIdAsync(command.LeadId, command.TenantId, Arg.Any<CancellationToken>())
            .Returns(lead);
        _contactReadRepository.GetByEmailAsync(lead.Email, TenantId, Arg.Any<CancellationToken>())
            .Returns(existingContact);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Contact.Phone.Should().Be("+34 650 123 456");
    }
}
