// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.ContactsApi.Application.Common.Interfaces;
using Propely.ContactsApi.Application.Contacts.Dtos;
using Propely.ContactsApi.Application.Contacts.Interfaces;
using Propely.ContactsApi.Domain.Common.Exceptions;

namespace Propely.ContactsApi.Application.Contacts.Commands.UpdateContact;

public sealed class UpdateContactCommandHandler : IRequestHandler<UpdateContactCommand, ContactDto>
{
    private readonly IContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateContactCommandHandler(IContactRepository contactRepository, IUnitOfWork unitOfWork)
    {
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ContactDto> Handle(UpdateContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _contactRepository.GetByIdAsync(request.ContactId, request.TenantId, cancellationToken)
            ?? throw new NotFoundException($"Contact '{request.ContactId}' not found.");

        contact.Update(
            firstName: request.FirstName,
            lastName: request.LastName,
            email: request.Email,
            roles: request.Roles,
            phone: request.Phone,
            secondaryPhone: request.SecondaryPhone,
            company: request.Company,
            notes: request.Notes,
            preferredLanguage: request.PreferredLanguage,
            source: request.Source,
            assignedAgentId: request.AssignedAgentId);

        _contactRepository.Update(contact);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ContactMapper.ToDto(contact);
    }
}
