// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Application.Common.Interfaces;
using Propely.AppointmentsApi.Domain.Common.Exceptions;

namespace Propely.AppointmentsApi.Application.Appointments.Commands.DisconnectCalendar;

public sealed class DisconnectCalendarCommandHandler : IRequestHandler<DisconnectCalendarCommand, Unit>
{
    private readonly ICalendarConnectionRepository _connectionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DisconnectCalendarCommandHandler(
        ICalendarConnectionRepository connectionRepository,
        IUnitOfWork unitOfWork)
    {
        _connectionRepository = connectionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DisconnectCalendarCommand request, CancellationToken cancellationToken)
    {
        var connection = await _connectionRepository.GetByAgentAndProviderAsync(
            request.AgentId, request.TenantId, request.Provider, cancellationToken);

        if (connection is null)
            throw new NotFoundException($"No {request.Provider} calendar connection found for this agent.");

        connection.SoftDelete();
        _connectionRepository.Update(connection);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
