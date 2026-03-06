// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentValidation;
using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.Application.Appointments.Commands.CreateAppointment;

public sealed class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(Appointment.TitleMaxLength);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.StartTimeUtc).NotEmpty();
        RuleFor(x => x.EndTimeUtc).NotEmpty()
            .GreaterThan(x => x.StartTimeUtc).WithMessage("End time must be after start time.");
        RuleFor(x => x.AgentId).NotEmpty();
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(Appointment.DescriptionMaxLength);
        RuleFor(x => x.Location).MaximumLength(Appointment.LocationMaxLength);
        RuleFor(x => x.Notes).MaximumLength(Appointment.NotesMaxLength);
        RuleFor(x => x.PropertyId)
            .NotEmpty()
            .When(x => x.Type == AppointmentType.PropertyViewing)
            .WithMessage("Property ID is required for property viewing appointments.");
    }
}
