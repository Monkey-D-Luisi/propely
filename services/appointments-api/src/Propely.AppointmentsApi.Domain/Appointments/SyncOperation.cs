// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Domain.Common;
using Propely.AppointmentsApi.Domain.Common.Exceptions;

namespace Propely.AppointmentsApi.Domain.Appointments;

public sealed class SyncOperation : Entity
{
    public const int ErrorMessageMaxLength = 2000;

    public Guid Id { get; private set; }
    public Guid CalendarConnectionId { get; private set; }
    public Guid? AppointmentId { get; private set; }
    public SyncDirection Direction { get; private set; }
    public SyncOperationType OperationType { get; private set; }
    public SyncOperationStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime? StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private SyncOperation() { }

    public static SyncOperation Create(
        Guid calendarConnectionId,
        Guid? appointmentId,
        SyncDirection direction,
        SyncOperationType operationType)
    {
        if (calendarConnectionId == Guid.Empty)
            throw new DomainException("Calendar connection ID is required.");

        return new SyncOperation
        {
            Id = Guid.NewGuid(),
            CalendarConnectionId = calendarConnectionId,
            AppointmentId = appointmentId,
            Direction = direction,
            OperationType = operationType,
            Status = SyncOperationStatus.Pending,
            RetryCount = 0,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Start()
    {
        if (Status != SyncOperationStatus.Pending)
            throw new DomainException($"Cannot start a sync operation in '{Status}' status.");

        Status = SyncOperationStatus.InProgress;
        StartedAtUtc = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != SyncOperationStatus.InProgress)
            throw new DomainException($"Cannot complete a sync operation in '{Status}' status.");

        Status = SyncOperationStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;
    }

    public void Fail(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new DomainException("Error message is required.");

        var truncated = errorMessage.Length > ErrorMessageMaxLength
            ? errorMessage[..ErrorMessageMaxLength]
            : errorMessage;

        Status = SyncOperationStatus.Failed;
        ErrorMessage = truncated;
        CompletedAtUtc = DateTime.UtcNow;
    }

    public void IncrementRetry()
    {
        RetryCount++;
        Status = SyncOperationStatus.Pending;
        ErrorMessage = null;
        StartedAtUtc = null;
        CompletedAtUtc = null;
    }
}
