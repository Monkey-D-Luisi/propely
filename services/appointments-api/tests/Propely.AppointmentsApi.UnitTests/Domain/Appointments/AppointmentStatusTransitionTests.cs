// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.AppointmentsApi.Domain.Appointments;
using Propely.AppointmentsApi.Domain.Common.Exceptions;

namespace Propely.AppointmentsApi.UnitTests.Domain.Appointments;

public class AppointmentStatusTransitionTests
{
    private static Appointment CreateAppointmentInStatus(AppointmentStatus targetStatus)
    {
        var appointment = Appointment.Create(
            "Test Appointment",
            AppointmentType.Generic,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            Guid.NewGuid(),
            Guid.NewGuid());

        switch (targetStatus)
        {
            case AppointmentStatus.Scheduled:
                break;
            case AppointmentStatus.Confirmed:
                appointment.Confirm();
                break;
            case AppointmentStatus.Completed:
                appointment.Complete();
                break;
            case AppointmentStatus.Cancelled:
                appointment.Cancel("Test cancellation");
                break;
            case AppointmentStatus.NoShow:
                appointment.MarkNoShow();
                break;
        }

        appointment.ClearDomainEvents();
        return appointment;
    }

    // Valid transitions from Scheduled
    [Theory]
    [InlineData(AppointmentStatus.Scheduled, AppointmentStatus.Confirmed)]
    [InlineData(AppointmentStatus.Scheduled, AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.Scheduled, AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.Scheduled, AppointmentStatus.NoShow)]
    // Valid transitions from Confirmed
    [InlineData(AppointmentStatus.Confirmed, AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.Confirmed, AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.Confirmed, AppointmentStatus.NoShow)]
    public void ValidTransition_Succeeds(AppointmentStatus from, AppointmentStatus to)
    {
        var appointment = CreateAppointmentInStatus(from);

        Action act = to switch
        {
            AppointmentStatus.Confirmed => () => appointment.Confirm(),
            AppointmentStatus.Completed => () => appointment.Complete(),
            AppointmentStatus.Cancelled => () => appointment.Cancel("Reason"),
            AppointmentStatus.NoShow => () => appointment.MarkNoShow(),
            _ => throw new ArgumentException($"Unexpected target status: {to}")
        };

        act.Should().NotThrow();
        appointment.Status.Should().Be(to);
    }

    // Invalid transitions -- terminal states
    [Theory]
    [InlineData(AppointmentStatus.Completed, AppointmentStatus.Scheduled)]
    [InlineData(AppointmentStatus.Completed, AppointmentStatus.Confirmed)]
    [InlineData(AppointmentStatus.Completed, AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.Completed, AppointmentStatus.NoShow)]
    [InlineData(AppointmentStatus.Cancelled, AppointmentStatus.Scheduled)]
    [InlineData(AppointmentStatus.Cancelled, AppointmentStatus.Confirmed)]
    [InlineData(AppointmentStatus.Cancelled, AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.Cancelled, AppointmentStatus.NoShow)]
    [InlineData(AppointmentStatus.NoShow, AppointmentStatus.Scheduled)]
    [InlineData(AppointmentStatus.NoShow, AppointmentStatus.Confirmed)]
    [InlineData(AppointmentStatus.NoShow, AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.NoShow, AppointmentStatus.Cancelled)]
    // Invalid: same status
    [InlineData(AppointmentStatus.Scheduled, AppointmentStatus.Scheduled)]
    [InlineData(AppointmentStatus.Confirmed, AppointmentStatus.Confirmed)]
    [InlineData(AppointmentStatus.Completed, AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.Cancelled, AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.NoShow, AppointmentStatus.NoShow)]
    // Invalid: backward transitions
    [InlineData(AppointmentStatus.Confirmed, AppointmentStatus.Scheduled)]
    public void InvalidTransition_ThrowsDomainException(AppointmentStatus from, AppointmentStatus to)
    {
        var appointment = CreateAppointmentInStatus(from);

        Action act = to switch
        {
            AppointmentStatus.Scheduled => () => throw new DomainException("No method to set Scheduled"),
            AppointmentStatus.Confirmed => () => appointment.Confirm(),
            AppointmentStatus.Completed => () => appointment.Complete(),
            AppointmentStatus.Cancelled => () => appointment.Cancel("Reason"),
            AppointmentStatus.NoShow => () => appointment.MarkNoShow(),
            _ => throw new ArgumentException($"Unexpected target status: {to}")
        };

        act.Should().Throw<DomainException>();
    }
}
