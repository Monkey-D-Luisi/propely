// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Agencies.Interfaces;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Domain.Agencies;
using Propely.OrgsApi.Domain.Common.Exceptions;
using MediatR;

namespace Propely.OrgsApi.Application.Agencies.Commands.CreateAgency;

public sealed class CreateAgencyCommandHandler : IRequestHandler<CreateAgencyCommand, CreateAgencyResult>
{
    private readonly IAgencyRepository _agencyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAgencyCommandHandler(
        IAgencyRepository agencyRepository,
        IUnitOfWork unitOfWork)
    {
        _agencyRepository = agencyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateAgencyResult> Handle(CreateAgencyCommand request, CancellationToken cancellationToken)
    {
        if (await _agencyRepository.ExistsBySlugAsync(request.Slug, cancellationToken: cancellationToken))
            throw new ConflictException("An agency with this slug already exists.");

        var agency = Agency.Create(request.Name, request.Slug, request.CreatedByUserId);

        await _agencyRepository.AddAsync(agency, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateAgencyResult(agency.Id, agency.Name, agency.Slug.Value);
    }
}
