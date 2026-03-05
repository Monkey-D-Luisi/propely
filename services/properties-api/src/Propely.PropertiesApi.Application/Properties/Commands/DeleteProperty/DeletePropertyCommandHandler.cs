// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.PropertiesApi.Application.Common.Interfaces;
using Propely.PropertiesApi.Application.Properties.Interfaces;
using Propely.PropertiesApi.Domain.Common.Exceptions;

namespace Propely.PropertiesApi.Application.Properties.Commands.DeleteProperty;

public sealed class DeletePropertyCommandHandler : IRequestHandler<DeletePropertyCommand>
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePropertyCommandHandler(
        IPropertyRepository propertyRepository,
        IUnitOfWork unitOfWork)
    {
        _propertyRepository = propertyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeletePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = await _propertyRepository.GetByIdAsync(request.PropertyId, request.TenantId, cancellationToken)
            ?? throw new NotFoundException($"Property with ID '{request.PropertyId}' was not found.");

        property.SoftDelete();
        _propertyRepository.Update(property);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
