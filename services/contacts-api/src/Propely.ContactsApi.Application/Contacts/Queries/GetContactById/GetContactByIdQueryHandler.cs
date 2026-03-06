// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.ContactsApi.Application.Contacts.Dtos;
using Propely.ContactsApi.Application.Contacts.Interfaces;

namespace Propely.ContactsApi.Application.Contacts.Queries.GetContactById;

public sealed class GetContactByIdQueryHandler : IRequestHandler<GetContactByIdQuery, ContactDto?>
{
    private readonly IContactReadRepository _contactReadRepository;

    public GetContactByIdQueryHandler(IContactReadRepository contactReadRepository)
    {
        _contactReadRepository = contactReadRepository;
    }

    public async Task<ContactDto?> Handle(GetContactByIdQuery request, CancellationToken cancellationToken)
    {
        var contact = await _contactReadRepository.GetByIdAsync(request.ContactId, request.TenantId, cancellationToken);
        return contact is null ? null : ContactMapper.ToDto(contact);
    }
}
