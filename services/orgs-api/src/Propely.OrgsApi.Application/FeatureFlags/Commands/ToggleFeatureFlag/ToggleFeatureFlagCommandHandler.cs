// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.FeatureFlags.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.FeatureFlags;

namespace Propely.OrgsApi.Application.FeatureFlags.Commands.ToggleFeatureFlag;

public sealed class ToggleFeatureFlagCommandHandler : IRequestHandler<ToggleFeatureFlagCommand>
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFeatureFlagDefaults _defaults;

    public ToggleFeatureFlagCommandHandler(
        IFeatureFlagRepository repository,
        IUnitOfWork unitOfWork,
        IFeatureFlagDefaults defaults)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _defaults = defaults;
    }

    public async Task Handle(ToggleFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        if (!_defaults.IsDefinedFlag(request.Name))
            throw new NotFoundException($"Feature flag '{request.Name}' is not defined.");

        var existing = await _repository.GetByNameAsync(request.Name, cancellationToken);

        if (existing is not null)
        {
            existing.SetEnabled(request.IsEnabled);
        }
        else
        {
            var flag = FeatureFlag.Create(request.Name, request.IsEnabled);
            await _repository.AddAsync(flag, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
