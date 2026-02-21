// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Agencies.DTOs;
using Propely.OrgsApi.Application.Agencies.Interfaces;
using MediatR;

namespace Propely.OrgsApi.Application.Agencies.Queries.ListAgenciesForUser;

public sealed class ListAgenciesForUserQueryHandler : IRequestHandler<ListAgenciesForUserQuery, List<AgencyDto>>
{
    private readonly IAgencyRepository _agencyRepository;

    public ListAgenciesForUserQueryHandler(IAgencyRepository agencyRepository)
    {
        _agencyRepository = agencyRepository;
    }

    public async Task<List<AgencyDto>> Handle(ListAgenciesForUserQuery request, CancellationToken cancellationToken)
    {
        var agencies = await _agencyRepository.ListForUserAsync(request.UserId, cancellationToken);

        return agencies.Select(a => new AgencyDto(
            a.Id,
            a.Name,
            a.Slug.Value,
            a.CreatedAtUtc,
            a.BranchIds.Count)).ToList();
    }
}
