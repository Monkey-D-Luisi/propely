// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.PropertiesApi.Application.Properties.Interfaces;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.Application.Properties.Queries.CountPropertiesByStatus;

public sealed record CountPropertiesByStatusQuery(Guid TenantId) : IRequest<Dictionary<string, int>>;

public sealed class CountPropertiesByStatusQueryHandler : IRequestHandler<CountPropertiesByStatusQuery, Dictionary<string, int>>
{
    private readonly IPropertyReadRepository _readRepository;

    public CountPropertiesByStatusQueryHandler(IPropertyReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Dictionary<string, int>> Handle(CountPropertiesByStatusQuery request, CancellationToken cancellationToken)
    {
        var counts = await _readRepository.CountByStatusAsync(request.TenantId, cancellationToken);

        return counts.ToDictionary(
            kvp => kvp.Key.ToString(),
            kvp => kvp.Value);
    }
}
