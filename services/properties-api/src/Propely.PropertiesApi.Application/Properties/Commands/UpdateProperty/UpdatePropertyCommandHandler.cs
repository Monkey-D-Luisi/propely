// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.PropertiesApi.Application.Common.Interfaces;
using Propely.PropertiesApi.Application.Properties.Dtos;
using Propely.PropertiesApi.Application.Properties.Interfaces;
using Propely.PropertiesApi.Domain.Common.Exceptions;

namespace Propely.PropertiesApi.Application.Properties.Commands.UpdateProperty;

public sealed class UpdatePropertyCommandHandler : IRequestHandler<UpdatePropertyCommand, PropertyDto>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePropertyCommandHandler(
        IPropertyRepository propertyRepository,
        IUnitOfWork unitOfWork)
    {
        _propertyRepository = propertyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PropertyDto> Handle(UpdatePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(request.PropertyId, request.TenantId, cancellationToken)
            ?? throw new NotFoundException($"Property with ID '{request.PropertyId}' was not found.");

        property.Update(
            title: request.Title,
            propertyType: request.PropertyType,
            operationType: request.OperationType,
            description: PropertyMapper.ToLocalizedText(request.Description),
            address: PropertyMapper.ToAddress(request.Address),
            features: PropertyMapper.ToFeatures(request.Features),
            financials: PropertyMapper.ToFinancials(request.Financials),
            virtualTourUrl: request.VirtualTourUrl,
            videoUrl: request.VideoUrl,
            updatedBy: request.UpdatedBy);

        _propertyRepository.Update(property);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PropertyMapper.ToDto(property);
    }
}
