// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AppointmentsApi.Application.Appointments.Dtos;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Application.Common.Interfaces;
using Propely.AppointmentsApi.Domain.Appointments;
using Propely.AppointmentsApi.Domain.Common.Exceptions;

namespace Propely.AppointmentsApi.Application.Appointments.Commands.ConnectCalendar;

public sealed class ConnectCalendarCommandHandler : IRequestHandler<ConnectCalendarCommand, CalendarConnectionDto>
{
    private readonly ICalendarTokenExchangeService _tokenExchangeService;
    private readonly ITokenEncryptionService _encryptionService;
    private readonly ICalendarConnectionRepository _connectionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConnectCalendarCommandHandler(
        ICalendarTokenExchangeService tokenExchangeService,
        ITokenEncryptionService encryptionService,
        ICalendarConnectionRepository connectionRepository,
        IUnitOfWork unitOfWork)
    {
        _tokenExchangeService = tokenExchangeService;
        _encryptionService = encryptionService;
        _connectionRepository = connectionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CalendarConnectionDto> Handle(ConnectCalendarCommand request, CancellationToken cancellationToken)
    {
        // Check if a connection already exists for this agent+provider
        var existing = await _connectionRepository.GetByAgentAndProviderAsync(
            request.AgentId, request.TenantId, request.Provider, cancellationToken);

        if (existing is not null)
            throw new ConflictException($"A {request.Provider} calendar connection already exists for this agent.");

        // Exchange authorization code for tokens
        var tokenResult = await _tokenExchangeService.ExchangeCodeAsync(
            request.Provider, request.AuthorizationCode, request.RedirectUri, cancellationToken);

        // Encrypt tokens
        var encryptedAccessToken = _encryptionService.Encrypt(tokenResult.AccessToken);
        var encryptedRefreshToken = _encryptionService.Encrypt(tokenResult.RefreshToken);

        // Create calendar connection
        var connection = CalendarConnection.Create(
            agentId: request.AgentId,
            tenantId: request.TenantId,
            provider: request.Provider,
            encryptedAccessToken: encryptedAccessToken,
            encryptedRefreshToken: encryptedRefreshToken,
            tokenExpiresAtUtc: tokenResult.ExpiresAtUtc,
            externalCalendarId: tokenResult.CalendarId);

        await _connectionRepository.AddAsync(connection, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CalendarConnectionMapper.ToDto(connection);
    }
}
