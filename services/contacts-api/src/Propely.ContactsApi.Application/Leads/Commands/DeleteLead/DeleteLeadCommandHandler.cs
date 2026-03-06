// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.ContactsApi.Application.Common.Interfaces;
using Propely.ContactsApi.Application.Leads.Interfaces;
using Propely.ContactsApi.Domain.Common.Exceptions;

namespace Propely.ContactsApi.Application.Leads.Commands.DeleteLead;

public sealed class DeleteLeadCommandHandler : IRequestHandler<DeleteLeadCommand>
{
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteLeadCommandHandler(ILeadRepository leadRepository, IUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(request.LeadId, request.TenantId, cancellationToken)
            ?? throw new NotFoundException($"Lead '{request.LeadId}' not found.");

        lead.SoftDelete();
        _leadRepository.Update(lead);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
