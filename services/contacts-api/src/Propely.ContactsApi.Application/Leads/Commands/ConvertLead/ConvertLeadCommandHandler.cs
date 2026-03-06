// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.ContactsApi.Application.Common.Interfaces;
using Propely.ContactsApi.Application.Contacts.Dtos;
using Propely.ContactsApi.Application.Contacts.Interfaces;
using Propely.ContactsApi.Application.Leads.Dtos;
using Propely.ContactsApi.Application.Leads.Interfaces;
using Propely.ContactsApi.Domain.Common.Exceptions;
using Propely.ContactsApi.Domain.Contacts;
using Propely.ContactsApi.Domain.Leads;

namespace Propely.ContactsApi.Application.Leads.Commands.ConvertLead;

public sealed class ConvertLeadCommandHandler : IRequestHandler<ConvertLeadCommand, ConvertLeadResponse>
{
    private readonly ILeadRepository _leadRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IContactReadRepository _contactReadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConvertLeadCommandHandler(
        ILeadRepository leadRepository,
        IContactRepository contactRepository,
        IContactReadRepository contactReadRepository,
        IUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _contactRepository = contactRepository;
        _contactReadRepository = contactReadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ConvertLeadResponse> Handle(ConvertLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(request.LeadId, request.TenantId, cancellationToken)
            ?? throw new NotFoundException($"Lead '{request.LeadId}' not found.");

        var interestType = request.Role switch
        {
            ContactRole.Buyer => InterestType.Buying,
            ContactRole.Tenant => InterestType.Renting,
            _ => InterestType.Buying
        };

        var existingContact = await _contactReadRepository.GetByEmailAsync(
            lead.Email, request.TenantId, cancellationToken);

        Contact contact;
        bool wasNewContact;

        if (existingContact is not null)
        {
            contact = existingContact;
            wasNewContact = false;

            contact.AddPropertyInterest(lead.PropertyId, interestType, request.Notes);

            if (lead.Phone is not null && contact.Phone is null)
            {
                contact.Update(phone: lead.Phone);
            }

            if (lead.AssignedAgentId.HasValue && !contact.AssignedAgentId.HasValue)
            {
                contact.Update(assignedAgentId: lead.AssignedAgentId);
            }

            _contactRepository.Update(contact);
        }
        else
        {
            var (firstName, lastName) = SplitName(lead.Name);

            contact = Contact.Create(
                firstName: firstName,
                lastName: lastName,
                email: lead.Email,
                tenantId: request.TenantId,
                roles: [request.Role],
                phone: lead.Phone,
                notes: request.Notes,
                source: ContactSource.Portal,
                assignedAgentId: lead.AssignedAgentId);

            contact.AddPropertyInterest(lead.PropertyId, interestType, request.Notes);

            await _contactRepository.AddAsync(contact, cancellationToken);
            wasNewContact = true;
        }

        lead.Convert(contact.Id, wasNewContact);
        _leadRepository.Update(lead);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ConvertLeadResponse
        {
            Lead = LeadMapper.ToDto(lead),
            Contact = ContactMapper.ToDto(contact),
            WasNewContact = wasNewContact
        };
    }

    private static (string FirstName, string LastName) SplitName(string fullName)
    {
        var parts = fullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => ("Unknown", "Unknown"),
            1 => (parts[0], parts[0]),
            _ => (parts[0], parts[1])
        };
    }
}
