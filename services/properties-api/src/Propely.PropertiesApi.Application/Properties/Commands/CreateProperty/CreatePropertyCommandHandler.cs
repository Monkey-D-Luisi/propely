// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.PropertiesApi.Application.Common.Interfaces;
using Propely.PropertiesApi.Application.Properties.Dtos;
using Propely.PropertiesApi.Application.Properties.Interfaces;
using Propely.PropertiesApi.Domain.Properties;

namespace Propely.PropertiesApi.Application.Properties.Commands.CreateProperty;

public sealed class CreatePropertyCommandHandler : IRequestHandler<CreatePropertyCommand, PropertyDto>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePropertyCommandHandler(
        IPropertyRepository propertyRepository,
        IUnitOfWork unitOfWork)
    {
        _propertyRepository = propertyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PropertyDto> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = Property.Create(
            title: request.Title,
            propertyType: request.PropertyType,
            operationType: request.OperationType,
            tenantId: request.TenantId,
            agentId: request.AgentId,
            agencyId: request.AgencyId,
            description: PropertyMapper.ToLocalizedText(request.Description),
            address: PropertyMapper.ToAddress(request.Address),
            features: PropertyMapper.ToFeatures(request.Features),
            financials: PropertyMapper.ToFinancials(request.Financials),
            virtualTourUrl: request.VirtualTourUrl,
            videoUrl: request.VideoUrl,
            createdBy: request.AgentId);

        await _propertyRepository.AddAsync(property, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PropertyMapper.ToDto(property);
    }
}
