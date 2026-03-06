// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AppointmentsApi.Application.Appointments.Dtos;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;
using Propely.AppointmentsApi.Application.Common.Interfaces;
using Propely.AppointmentsApi.Domain.Common.Exceptions;

namespace Propely.AppointmentsApi.Application.Appointments.Commands.UpdateAppointment;

public sealed class UpdateAppointmentCommandHandler : IRequestHandler<UpdateAppointmentCommand, AppointmentDto>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAppointmentCommandHandler(IAppointmentRepository appointmentRepository, IUnitOfWork unitOfWork)
    {
        _appointmentRepository = appointmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AppointmentDto> Handle(UpdateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, request.TenantId, cancellationToken)
            ?? throw new NotFoundException($"Appointment '{request.AppointmentId}' not found.");

        appointment.Update(
            title: request.Title,
            startTimeUtc: request.StartTimeUtc,
            endTimeUtc: request.EndTimeUtc,
            description: request.Description,
            location: request.Location,
            isAllDay: request.IsAllDay,
            propertyId: request.PropertyId,
            contactId: request.ContactId,
            notes: request.Notes);

        _appointmentRepository.Update(appointment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AppointmentMapper.ToDto(appointment);
    }
}
