// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.AppointmentsApi.Domain.Appointments;
using Propely.AppointmentsApi.Domain.Appointments.Events;
using Propely.AppointmentsApi.Domain.Appointments.Exceptions;
using Propely.AppointmentsApi.Domain.Common.Exceptions;

namespace Propely.AppointmentsApi.UnitTests.Domain.Appointments;

public class AppointmentTests
{
    private static readonly Guid ValidAgentId = Guid.NewGuid();
    private static readonly Guid ValidTenantId = Guid.NewGuid();
    private static readonly Guid ValidPropertyId = Guid.NewGuid();
    private static readonly Guid ValidContactId = Guid.NewGuid();

    private static Appointment CreateValidAppointment(
        string title = "Property Viewing - Calle Mayor",
        AppointmentType type = AppointmentType.Generic,
        DateTime? startTimeUtc = null,
        DateTime? endTimeUtc = null,
        Guid? agentId = null,
        Guid? tenantId = null,
        string? description = null,
        string? location = null,
        bool isAllDay = false,
        Guid? propertyId = null,
        Guid? contactId = null,
        string? notes = null)
    {
        var start = startTimeUtc ?? DateTime.UtcNow.AddDays(1);
        var end = endTimeUtc ?? start.AddHours(1);

        return Appointment.Create(
            title, type, start, end,
            agentId ?? ValidAgentId,
            tenantId ?? ValidTenantId,
            description, location, isAllDay,
            propertyId, contactId, notes);
    }

    [Fact]
    public void Create_WithValidData_ReturnsAppointmentWithScheduledStatus()
    {
        var appointment = CreateValidAppointment();

        appointment.Should().NotBeNull();
        appointment.Id.Should().NotBeEmpty();
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        appointment.IsDeleted.Should().BeFalse();
        appointment.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithValidData_RaisesAppointmentCreatedV1Event()
    {
        var appointment = CreateValidAppointment();

        appointment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AppointmentCreatedV1>();

        var evt = (AppointmentCreatedV1)appointment.DomainEvents.First();
        evt.Data.AppointmentId.Should().Be(appointment.Id);
        evt.Data.AgentId.Should().Be(appointment.AgentId);
        evt.Data.TenantId.Should().Be(appointment.TenantId);
        evt.Data.Type.Should().Be(appointment.Type);
        evt.EventType.Should().Be(nameof(AppointmentCreatedV1));
        evt.SchemaVersion.Should().Be(1);
        evt.Producer.Should().Be("AppointmentsApi");
        evt.EventId.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithPropertyViewingType_RequiresPropertyId()
    {
        var act = () => CreateValidAppointment(type: AppointmentType.PropertyViewing, propertyId: null);

        act.Should().Throw<AppointmentValidationException>()
            .WithMessage("*Property ID*required*property viewing*");
    }

    [Fact]
    public void Create_WithPropertyViewingAndPropertyId_Succeeds()
    {
        var appointment = CreateValidAppointment(
            type: AppointmentType.PropertyViewing,
            propertyId: ValidPropertyId);

        appointment.Type.Should().Be(AppointmentType.PropertyViewing);
        appointment.PropertyId.Should().Be(ValidPropertyId);
    }

    [Fact]
    public void Create_WithGenericType_DoesNotRequirePropertyId()
    {
        var appointment = CreateValidAppointment(type: AppointmentType.Generic, propertyId: null);

        appointment.Type.Should().Be(AppointmentType.Generic);
        appointment.PropertyId.Should().BeNull();
    }

    [Fact]
    public void Create_WithOwnerMeetingType_DoesNotRequirePropertyId()
    {
        var appointment = CreateValidAppointment(type: AppointmentType.OwnerMeeting, propertyId: null);

        appointment.Type.Should().Be(AppointmentType.OwnerMeeting);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyTitle_ThrowsValidationException(string? title)
    {
        var act = () => CreateValidAppointment(title: title!);

        act.Should().Throw<AppointmentValidationException>()
            .WithMessage("*Title*required*");
    }

    [Fact]
    public void Create_WithTitleExceedingMaxLength_ThrowsValidationException()
    {
        var title = new string('A', Appointment.TitleMaxLength + 1);

        var act = () => CreateValidAppointment(title: title);

        act.Should().Throw<AppointmentValidationException>()
            .WithMessage("*Title*must not exceed*");
    }

    [Fact]
    public void Create_WithEndTimeBeforeStartTime_ThrowsValidationException()
    {
        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddHours(-1);

        var act = () => CreateValidAppointment(startTimeUtc: start, endTimeUtc: end);

        act.Should().Throw<AppointmentValidationException>()
            .WithMessage("*End time*after*start time*");
    }

    [Fact]
    public void Create_WithEqualStartAndEndTime_ThrowsValidationException()
    {
        var time = DateTime.UtcNow.AddDays(1);

        var act = () => CreateValidAppointment(startTimeUtc: time, endTimeUtc: time);

        act.Should().Throw<AppointmentValidationException>()
            .WithMessage("*End time*after*start time*");
    }

    [Fact]
    public void Create_WithEmptyAgentId_ThrowsValidationException()
    {
        var act = () => CreateValidAppointment(agentId: Guid.Empty);

        act.Should().Throw<AppointmentValidationException>()
            .WithMessage("*Agent ID*required*");
    }

    [Fact]
    public void Create_WithEmptyTenantId_ThrowsValidationException()
    {
        var act = () => CreateValidAppointment(tenantId: Guid.Empty);

        act.Should().Throw<AppointmentValidationException>()
            .WithMessage("*Tenant ID*required*");
    }

    [Fact]
    public void Create_WithOptionalFields_SetsAllFieldsCorrectly()
    {
        var appointment = CreateValidAppointment(
            description: "Meeting with client",
            location: "123 Main St",
            contactId: ValidContactId,
            notes: "Bring documents");

        appointment.Description.Should().Be("Meeting with client");
        appointment.Location.Should().Be("123 Main St");
        appointment.ContactId.Should().Be(ValidContactId);
        appointment.Notes.Should().Be("Bring documents");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var appointment = CreateValidAppointment(
            title: "  Some Title  ",
            description: "  Desc  ",
            location: "  Loc  ",
            notes: "  Notes  ");

        appointment.Title.Should().Be("Some Title");
        appointment.Description.Should().Be("Desc");
        appointment.Location.Should().Be("Loc");
        appointment.Notes.Should().Be("Notes");
    }

    [Fact]
    public void Create_WithDescriptionExceedingMaxLength_ThrowsValidationException()
    {
        var desc = new string('A', Appointment.DescriptionMaxLength + 1);

        var act = () => CreateValidAppointment(description: desc);

        act.Should().Throw<AppointmentValidationException>()
            .WithMessage("*Description*must not exceed*");
    }

    [Fact]
    public void Create_WithLocationExceedingMaxLength_ThrowsValidationException()
    {
        var loc = new string('A', Appointment.LocationMaxLength + 1);

        var act = () => CreateValidAppointment(location: loc);

        act.Should().Throw<AppointmentValidationException>()
            .WithMessage("*Location*must not exceed*");
    }

    [Fact]
    public void Create_WithNotesExceedingMaxLength_ThrowsValidationException()
    {
        var notes = new string('A', Appointment.NotesMaxLength + 1);

        var act = () => CreateValidAppointment(notes: notes);

        act.Should().Throw<AppointmentValidationException>()
            .WithMessage("*Notes*must not exceed*");
    }

    // --- Update tests ---

    [Fact]
    public void Update_WithValidData_UpdatesFieldsAndRaisesEvent()
    {
        var appointment = CreateValidAppointment();
        appointment.ClearDomainEvents();

        var newStart = DateTime.UtcNow.AddDays(2);
        var newEnd = newStart.AddHours(2);

        appointment.Update("Updated Title", newStart, newEnd, "New desc", "New loc");

        appointment.Title.Should().Be("Updated Title");
        appointment.StartTimeUtc.Should().Be(newStart);
        appointment.EndTimeUtc.Should().Be(newEnd);
        appointment.Description.Should().Be("New desc");
        appointment.Location.Should().Be("New loc");
        appointment.UpdatedAtUtc.Should().NotBeNull();
        appointment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AppointmentUpdatedV1>();
    }

    [Fact]
    public void Update_CancelledAppointment_ThrowsDomainException()
    {
        var appointment = CreateValidAppointment();
        appointment.Cancel("Test reason");
        appointment.ClearDomainEvents();

        var act = () => appointment.Update("New Title", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1));

        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot update*Cancelled*");
    }

    [Fact]
    public void Update_CompletedAppointment_ThrowsDomainException()
    {
        var appointment = CreateValidAppointment();
        appointment.Complete();
        appointment.ClearDomainEvents();

        var act = () => appointment.Update("New Title", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1));

        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot update*Completed*");
    }

    // --- Confirm tests ---

    [Fact]
    public void Confirm_FromScheduled_SetsStatusToConfirmed()
    {
        var appointment = CreateValidAppointment();
        appointment.ClearDomainEvents();

        appointment.Confirm();

        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
        appointment.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Confirm_FromConfirmed_ThrowsDomainException()
    {
        var appointment = CreateValidAppointment();
        appointment.Confirm();

        var act = () => appointment.Confirm();

        act.Should().Throw<DomainException>()
            .WithMessage("*already*Confirmed*");
    }

    // --- Complete tests ---

    [Fact]
    public void Complete_FromScheduled_SetsStatusToCompleted()
    {
        var appointment = CreateValidAppointment();
        appointment.ClearDomainEvents();

        appointment.Complete("All went well");

        appointment.Status.Should().Be(AppointmentStatus.Completed);
        appointment.Notes.Should().Be("All went well");
    }

    [Fact]
    public void Complete_FromConfirmed_SetsStatusToCompleted()
    {
        var appointment = CreateValidAppointment();
        appointment.Confirm();
        appointment.ClearDomainEvents();

        appointment.Complete();

        appointment.Status.Should().Be(AppointmentStatus.Completed);
    }

    [Fact]
    public void Complete_RaisesCompletedEvent()
    {
        var appointment = CreateValidAppointment();
        appointment.ClearDomainEvents();

        appointment.Complete("Done");

        appointment.DomainEvents.Should().Contain(e => e is AppointmentCompletedV1);
        var evt = appointment.DomainEvents.OfType<AppointmentCompletedV1>().First();
        evt.Data.AppointmentId.Should().Be(appointment.Id);
        evt.Data.Notes.Should().Be("Done");
    }

    [Fact]
    public void Complete_FromCancelled_ThrowsDomainException()
    {
        var appointment = CreateValidAppointment();
        appointment.Cancel("No longer needed");

        var act = () => appointment.Complete();

        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot transition*Cancelled*Completed*");
    }

    // --- Cancel tests ---

    [Fact]
    public void Cancel_FromScheduled_SetsStatusToCancelled()
    {
        var appointment = CreateValidAppointment();
        appointment.ClearDomainEvents();

        appointment.Cancel("Client unavailable");

        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment.CancellationReason.Should().Be("Client unavailable");
    }

    [Fact]
    public void Cancel_RaisesCancelledEvent()
    {
        var appointment = CreateValidAppointment();
        appointment.ClearDomainEvents();

        appointment.Cancel("Test reason");

        appointment.DomainEvents.Should().Contain(e => e is AppointmentCancelledV1);
        var evt = appointment.DomainEvents.OfType<AppointmentCancelledV1>().First();
        evt.Data.AppointmentId.Should().Be(appointment.Id);
        evt.Data.Reason.Should().Be("Test reason");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Cancel_WithEmptyReason_ThrowsValidationException(string? reason)
    {
        var appointment = CreateValidAppointment();

        var act = () => appointment.Cancel(reason!);

        act.Should().Throw<AppointmentValidationException>()
            .WithMessage("*Cancellation reason*required*");
    }

    [Fact]
    public void Cancel_WithReasonExceedingMaxLength_ThrowsValidationException()
    {
        var appointment = CreateValidAppointment();
        var reason = new string('A', Appointment.CancellationReasonMaxLength + 1);

        var act = () => appointment.Cancel(reason);

        act.Should().Throw<AppointmentValidationException>()
            .WithMessage("*Cancellation reason*must not exceed*");
    }

    // --- MarkNoShow tests ---

    [Fact]
    public void MarkNoShow_FromScheduled_SetsStatusToNoShow()
    {
        var appointment = CreateValidAppointment();

        appointment.MarkNoShow();

        appointment.Status.Should().Be(AppointmentStatus.NoShow);
    }

    [Fact]
    public void MarkNoShow_FromConfirmed_SetsStatusToNoShow()
    {
        var appointment = CreateValidAppointment();
        appointment.Confirm();

        appointment.MarkNoShow();

        appointment.Status.Should().Be(AppointmentStatus.NoShow);
    }

    // --- CalendarSync tests ---

    [Fact]
    public void SetCalendarSync_AddsNewSyncInfo()
    {
        var appointment = CreateValidAppointment();

        appointment.SetCalendarSync("google-event-123", CalendarProvider.Google);

        appointment.CalendarSyncInfos.Should().ContainSingle();
        var info = appointment.CalendarSyncInfos.First();
        info.ExternalEventId.Should().Be("google-event-123");
        info.Provider.Should().Be(CalendarProvider.Google);
        info.LastSyncedUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void SetCalendarSync_UpdatesExistingSyncInfo()
    {
        var appointment = CreateValidAppointment();
        appointment.SetCalendarSync("google-event-123", CalendarProvider.Google);
        var originalSync = appointment.CalendarSyncInfos.First().LastSyncedUtc;

        appointment.SetCalendarSync("google-event-123-updated", CalendarProvider.Google);

        appointment.CalendarSyncInfos.Should().ContainSingle();
        appointment.CalendarSyncInfos.First().LastSyncedUtc.Should().BeOnOrAfter(originalSync);
    }

    [Fact]
    public void SetCalendarSync_MultipleProviders_TracksIndependently()
    {
        var appointment = CreateValidAppointment();

        appointment.SetCalendarSync("google-event-123", CalendarProvider.Google);
        appointment.SetCalendarSync("ms-event-456", CalendarProvider.Microsoft);

        appointment.CalendarSyncInfos.Should().HaveCount(2);
        appointment.CalendarSyncInfos.Should().Contain(s => s.Provider == CalendarProvider.Google);
        appointment.CalendarSyncInfos.Should().Contain(s => s.Provider == CalendarProvider.Microsoft);
    }

    // --- SoftDelete tests ---

    [Fact]
    public void SoftDelete_SetsIsDeletedAndTimestamps()
    {
        var appointment = CreateValidAppointment();

        appointment.SoftDelete();

        appointment.IsDeleted.Should().BeTrue();
        appointment.DeletedAtUtc.Should().NotBeNull();
        appointment.DeletedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        appointment.UpdatedAtUtc.Should().NotBeNull();
    }
}
