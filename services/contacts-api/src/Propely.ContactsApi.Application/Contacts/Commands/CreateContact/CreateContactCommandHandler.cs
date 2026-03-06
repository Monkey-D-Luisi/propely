// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.ContactsApi.Application.Common.Interfaces;
using Propely.ContactsApi.Application.Contacts.Dtos;
using Propely.ContactsApi.Application.Contacts.Interfaces;
using Propely.ContactsApi.Domain.Contacts;

namespace Propely.ContactsApi.Application.Contacts.Commands.CreateContact;

public sealed class CreateContactCommandHandler : IRequestHandler<CreateContactCommand, ContactDto>
{
    private readonly IContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateContactCommandHandler(IContactRepository contactRepository, IUnitOfWork unitOfWork)
    {
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ContactDto> Handle(CreateContactCommand request, CancellationToken cancellationToken)
    {
        var contact = Contact.Create(
            firstName: request.FirstName,
            lastName: request.LastName,
            email: request.Email,
            tenantId: request.TenantId,
            roles: request.Roles,
            phone: request.Phone,
            secondaryPhone: request.SecondaryPhone,
            company: request.Company,
            notes: request.Notes,
            preferredLanguage: request.PreferredLanguage,
            source: request.Source,
            assignedAgentId: request.AssignedAgentId);

        await _contactRepository.AddAsync(contact, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ContactMapper.ToDto(contact);
    }
}
