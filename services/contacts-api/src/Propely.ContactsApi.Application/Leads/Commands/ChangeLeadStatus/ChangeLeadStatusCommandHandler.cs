// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.ContactsApi.Application.Common.Interfaces;
using Propely.ContactsApi.Application.Leads.Dtos;
using Propely.ContactsApi.Application.Leads.Interfaces;
using Propely.ContactsApi.Domain.Common.Exceptions;

namespace Propely.ContactsApi.Application.Leads.Commands.ChangeLeadStatus;

public sealed class ChangeLeadStatusCommandHandler : IRequestHandler<ChangeLeadStatusCommand, LeadDto>
{
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeLeadStatusCommandHandler(ILeadRepository leadRepository, IUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeadDto> Handle(ChangeLeadStatusCommand request, CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(request.LeadId, request.TenantId, cancellationToken)
            ?? throw new NotFoundException($"Lead '{request.LeadId}' not found.");

        lead.ChangeStatus(request.NewStatus);
        _leadRepository.Update(lead);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LeadMapper.ToDto(lead);
    }
}
