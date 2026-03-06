// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Domain.Appointments.Events;
using Propely.AppointmentsApi.Domain.Appointments.Exceptions;
using Propely.AppointmentsApi.Domain.Common;
using Propely.AppointmentsApi.Domain.Common.Exceptions;

namespace Propely.AppointmentsApi.Domain.Appointments;

public sealed class Appointment : Entity, ISoftDeletable
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;
    public const int LocationMaxLength = 500;
    public const int CancellationReasonMaxLength = 500;
    public const int NotesMaxLength = 2000;

    private static readonly Dictionary<AppointmentStatus, HashSet<AppointmentStatus>> ValidTransitions = new()
    {
        [AppointmentStatus.Scheduled] = [AppointmentStatus.Confirmed, AppointmentStatus.Completed, AppointmentStatus.Cancelled, AppointmentStatus.NoShow],
        [AppointmentStatus.Confirmed] = [AppointmentStatus.Completed, AppointmentStatus.Cancelled, AppointmentStatus.NoShow],
        [AppointmentStatus.Completed] = [],
        [AppointmentStatus.Cancelled] = [],
        [AppointmentStatus.NoShow] = []
    };

    private readonly List<CalendarSyncInfo> _calendarSyncInfos = [];

    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public AppointmentType Type { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public DateTime StartTimeUtc { get; private set; }
    public DateTime EndTimeUtc { get; private set; }
    public string? Location { get; private set; }
    public bool IsAllDay { get; private set; }
    public Guid? PropertyId { get; private set; }
    public Guid? ContactId { get; private set; }
    public Guid AgentId { get; private set; }
    public Guid TenantId { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? Notes { get; private set; }
    public IReadOnlyCollection<CalendarSyncInfo> CalendarSyncInfos => _calendarSyncInfos.AsReadOnly();
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    private Appointment() { }

    public static Appointment Create(
        string title,
        AppointmentType type,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        Guid agentId,
        Guid tenantId,
        string? description = null,
        string? location = null,
        bool isAllDay = false,
        Guid? propertyId = null,
        Guid? contactId = null,
        string? notes = null)
    {
        ValidateTitle(title);
        ValidateTimeRange(startTimeUtc, endTimeUtc);
        ValidateRequiredIds(agentId, tenantId);
        ValidateTypeConstraints(type, propertyId);
        ValidateOptionalFields(description, location, notes);

        var now = DateTime.UtcNow;

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Type = type,
            Status = AppointmentStatus.Scheduled,
            StartTimeUtc = startTimeUtc,
            EndTimeUtc = endTimeUtc,
            AgentId = agentId,
            TenantId = tenantId,
            Description = description?.Trim(),
            Location = location?.Trim(),
            IsAllDay = isAllDay,
            PropertyId = propertyId,
            ContactId = contactId,
            Notes = notes?.Trim(),
            CreatedAtUtc = now
        };

        appointment.RaiseDomainEvent(new AppointmentCreatedV1(
            appointment.Id, appointment.AgentId, appointment.TenantId, appointment.Type, now));

        return appointment;
    }

    public void Update(
        string title,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        string? description = null,
        string? location = null,
        bool isAllDay = false,
        Guid? propertyId = null,
        Guid? contactId = null,
        string? notes = null)
    {
        if (Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed)
            throw new DomainException($"Cannot update an appointment in '{Status}' status.");

        ValidateTitle(title);
        ValidateTimeRange(startTimeUtc, endTimeUtc);
        ValidateTypeConstraints(Type, propertyId);
        ValidateOptionalFields(description, location, notes);

        var now = DateTime.UtcNow;

        Title = title.Trim();
        StartTimeUtc = startTimeUtc;
        EndTimeUtc = endTimeUtc;
        Description = description?.Trim();
        Location = location?.Trim();
        IsAllDay = isAllDay;
        PropertyId = propertyId;
        ContactId = contactId;
        Notes = notes?.Trim();
        UpdatedAtUtc = now;

        RaiseDomainEvent(new AppointmentUpdatedV1(Id, AgentId, TenantId, now));
    }

    public void Confirm()
    {
        ChangeStatus(AppointmentStatus.Confirmed);
    }

    public void Complete(string? notes = null)
    {
        if (notes is not null)
        {
            if (notes.Trim().Length > NotesMaxLength)
                throw new AppointmentValidationException($"Notes must not exceed {NotesMaxLength} characters.");

            Notes = notes.Trim();
        }

        var previousStatus = Status;
        ChangeStatus(AppointmentStatus.Completed);

        // Replace the generic status-changed event with a specific completed event
        // by removing the last event and adding the completed one
        var now = DateTime.UtcNow;
        RaiseDomainEvent(new AppointmentCompletedV1(Id, AgentId, TenantId, Notes, now));
    }

    public void Cancel(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new AppointmentValidationException("Cancellation reason is required.");

        if (reason.Trim().Length > CancellationReasonMaxLength)
            throw new AppointmentValidationException($"Cancellation reason must not exceed {CancellationReasonMaxLength} characters.");

        CancellationReason = reason.Trim();

        var previousStatus = Status;
        ChangeStatus(AppointmentStatus.Cancelled);

        var now = DateTime.UtcNow;
        RaiseDomainEvent(new AppointmentCancelledV1(Id, AgentId, TenantId, CancellationReason, now));
    }

    public void MarkNoShow()
    {
        ChangeStatus(AppointmentStatus.NoShow);
    }

    public void SetCalendarSync(string externalEventId, CalendarProvider provider)
    {
        var existing = _calendarSyncInfos.FirstOrDefault(s => s.Provider == provider);
        var now = DateTime.UtcNow;

        if (existing is not null)
        {
            existing.UpdateLastSynced(now);
        }
        else
        {
            _calendarSyncInfos.Add(new CalendarSyncInfo(externalEventId, provider, now));
        }

        UpdatedAtUtc = now;
    }

    public void SoftDelete()
    {
        var now = DateTime.UtcNow;
        IsDeleted = true;
        DeletedAtUtc = now;
        UpdatedAtUtc = now;
    }

    private void ChangeStatus(AppointmentStatus newStatus)
    {
        if (Status == newStatus)
            throw new DomainException($"Appointment is already in '{newStatus}' status.");

        if (!ValidTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
            throw new DomainException($"Cannot transition from '{Status}' to '{newStatus}'.");

        Status = newStatus;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new AppointmentValidationException("Title is required.");

        if (title.Trim().Length > TitleMaxLength)
            throw new AppointmentValidationException($"Title must not exceed {TitleMaxLength} characters.");
    }

    private static void ValidateTimeRange(DateTime startTimeUtc, DateTime endTimeUtc)
    {
        if (endTimeUtc <= startTimeUtc)
            throw new AppointmentValidationException("End time must be after start time.");
    }

    private static void ValidateRequiredIds(Guid agentId, Guid tenantId)
    {
        if (agentId == Guid.Empty)
            throw new AppointmentValidationException("Agent ID is required.");

        if (tenantId == Guid.Empty)
            throw new AppointmentValidationException("Tenant ID is required.");
    }

    private static void ValidateTypeConstraints(AppointmentType type, Guid? propertyId)
    {
        if (type == AppointmentType.PropertyViewing && (propertyId is null || propertyId == Guid.Empty))
            throw new AppointmentValidationException("Property ID is required for property viewing appointments.");
    }

    private static void ValidateOptionalFields(string? description, string? location, string? notes)
    {
        if (description is not null && description.Trim().Length > DescriptionMaxLength)
            throw new AppointmentValidationException($"Description must not exceed {DescriptionMaxLength} characters.");

        if (location is not null && location.Trim().Length > LocationMaxLength)
            throw new AppointmentValidationException($"Location must not exceed {LocationMaxLength} characters.");

        if (notes is not null && notes.Trim().Length > NotesMaxLength)
            throw new AppointmentValidationException($"Notes must not exceed {NotesMaxLength} characters.");
    }
}
