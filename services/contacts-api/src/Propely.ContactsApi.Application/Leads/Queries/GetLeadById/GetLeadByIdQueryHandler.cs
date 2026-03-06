// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.ContactsApi.Application.Leads.Dtos;
using Propely.ContactsApi.Application.Leads.Interfaces;

namespace Propely.ContactsApi.Application.Leads.Queries.GetLeadById;

public sealed class GetLeadByIdQueryHandler : IRequestHandler<GetLeadByIdQuery, LeadDto?>
{
    private readonly ILeadReadRepository _leadReadRepository;

    public GetLeadByIdQueryHandler(ILeadReadRepository leadReadRepository)
    {
        _leadReadRepository = leadReadRepository;
    }

    public async Task<LeadDto?> Handle(GetLeadByIdQuery request, CancellationToken cancellationToken)
    {
        var lead = await _leadReadRepository.GetByIdAsync(request.LeadId, request.TenantId, cancellationToken);
        return lead is null ? null : LeadMapper.ToDto(lead);
    }
}
