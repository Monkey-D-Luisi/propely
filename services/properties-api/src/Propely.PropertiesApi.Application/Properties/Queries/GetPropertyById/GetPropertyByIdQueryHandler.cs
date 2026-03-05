// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.PropertiesApi.Application.Properties.Dtos;
using Propely.PropertiesApi.Application.Properties.Interfaces;

namespace Propely.PropertiesApi.Application.Properties.Queries.GetPropertyById;

public sealed class GetPropertyByIdQueryHandler : IRequestHandler<GetPropertyByIdQuery, PropertyDto?>
{
    private readonly IPropertyReadRepository _readRepository;

    public GetPropertyByIdQueryHandler(IPropertyReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<PropertyDto?> Handle(GetPropertyByIdQuery request, CancellationToken cancellationToken)
    {
        var property = await _readRepository.GetByIdAsync(request.PropertyId, request.TenantId, cancellationToken);
        return property is null ? null : PropertyMapper.ToDto(property);
    }
}
