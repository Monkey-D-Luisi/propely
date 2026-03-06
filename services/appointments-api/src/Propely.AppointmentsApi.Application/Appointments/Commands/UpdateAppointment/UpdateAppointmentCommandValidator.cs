// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentValidation;
using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.Application.Appointments.Commands.UpdateAppointment;

public sealed class UpdateAppointmentCommandValidator : AbstractValidator<UpdateAppointmentCommand>
{
    public UpdateAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(Appointment.TitleMaxLength);
        RuleFor(x => x.StartTimeUtc).NotEmpty();
        RuleFor(x => x.EndTimeUtc).NotEmpty()
            .GreaterThan(x => x.StartTimeUtc).WithMessage("End time must be after start time.");
        RuleFor(x => x.Description).MaximumLength(Appointment.DescriptionMaxLength);
        RuleFor(x => x.Location).MaximumLength(Appointment.LocationMaxLength);
        RuleFor(x => x.Notes).MaximumLength(Appointment.NotesMaxLength);
    }
}
