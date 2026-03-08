// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.ContactsApi.Application.Leads.Interfaces;

namespace Propely.ContactsApi.Application.Leads.Queries.CountLeadsByStatus;

public sealed record CountLeadsByStatusQuery(Guid TenantId) : IRequest<Dictionary<string, int>>;

public sealed class CountLeadsByStatusQueryHandler : IRequestHandler<CountLeadsByStatusQuery, Dictionary<string, int>>
{
    private readonly ILeadReadRepository _readRepository;

    public CountLeadsByStatusQueryHandler(ILeadReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Dictionary<string, int>> Handle(CountLeadsByStatusQuery request, CancellationToken cancellationToken)
    {
        var counts = await _readRepository.CountByStatusAsync(request.TenantId, cancellationToken);

        return counts.ToDictionary(
            kvp => kvp.Key.ToString(),
            kvp => kvp.Value);
    }
}
