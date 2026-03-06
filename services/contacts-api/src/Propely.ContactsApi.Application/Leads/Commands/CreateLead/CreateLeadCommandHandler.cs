// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.ContactsApi.Application.Common.Interfaces;
using Propely.ContactsApi.Application.Leads.Dtos;
using Propely.ContactsApi.Application.Leads.Interfaces;
using Propely.ContactsApi.Domain.Common.Exceptions;
using Propely.ContactsApi.Domain.Leads;

namespace Propely.ContactsApi.Application.Leads.Commands.CreateLead;

public sealed class CreateLeadCommandHandler : IRequestHandler<CreateLeadCommand, LeadDto>
{
    private readonly ILeadRepository _leadRepository;
    private readonly ILeadReadRepository _leadReadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLeadCommandHandler(
        ILeadRepository leadRepository,
        ILeadReadRepository leadReadRepository,
        IUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _leadReadRepository = leadReadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeadDto> Handle(CreateLeadCommand request, CancellationToken cancellationToken)
    {
        var exists = await _leadReadRepository.ExistsByEmailAndPropertyAsync(
            request.Email.Trim().ToLowerInvariant(), request.PropertyId, request.TenantId, cancellationToken);

        if (exists)
            throw new ConflictException($"A lead with email '{request.Email}' already exists for property '{request.PropertyId}'.");

        var lead = Lead.Create(
            name: request.Name,
            email: request.Email,
            propertyId: request.PropertyId,
            tenantId: request.TenantId,
            phone: request.Phone,
            message: request.Message,
            source: request.Source,
            assignedAgentId: request.AssignedAgentId);

        await _leadRepository.AddAsync(lead, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LeadMapper.ToDto(lead);
    }
}
