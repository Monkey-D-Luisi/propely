// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.ContactsApi.Application.Common.Interfaces;
using Propely.ContactsApi.Application.Contacts.Interfaces;
using Propely.ContactsApi.Domain.Common.Exceptions;

namespace Propely.ContactsApi.Application.Contacts.Commands.DeleteContact;

public sealed class DeleteContactCommandHandler : IRequestHandler<DeleteContactCommand>
{
    private readonly IContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteContactCommandHandler(IContactRepository contactRepository, IUnitOfWork unitOfWork)
    {
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _contactRepository.GetByIdAsync(request.ContactId, request.TenantId, cancellationToken)
            ?? throw new NotFoundException($"Contact '{request.ContactId}' not found.");

        contact.SoftDelete();
        _contactRepository.Update(contact);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
