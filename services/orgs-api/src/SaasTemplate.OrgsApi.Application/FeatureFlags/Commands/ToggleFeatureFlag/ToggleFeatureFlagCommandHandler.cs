// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using SaasTemplate.OrgsApi.Application.Common.Interfaces;
using SaasTemplate.OrgsApi.Application.FeatureFlags.Interfaces;
using SaasTemplate.OrgsApi.Domain.Common.Exceptions;
using SaasTemplate.OrgsApi.Domain.FeatureFlags;

namespace SaasTemplate.OrgsApi.Application.FeatureFlags.Commands.ToggleFeatureFlag;

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
