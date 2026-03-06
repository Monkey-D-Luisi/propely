// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Domain.Common;

namespace Propely.AppointmentsApi.Domain.Appointments.Events;

public sealed record AppointmentCompletedV1 : IDomainEvent
{
    public Guid EventId { get; }
    public string EventType => nameof(AppointmentCompletedV1);
    public int SchemaVersion => 1;
    public DateTime OccurredAtUtc { get; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Producer => "AppointmentsApi";

    public AppointmentCompletedV1Data Data { get; }

    public AppointmentCompletedV1(Guid appointmentId, Guid agentId, Guid tenantId, string? notes, DateTime occurredAtUtc)
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = occurredAtUtc;
        Data = new AppointmentCompletedV1Data(appointmentId, agentId, tenantId, notes);
    }
}

public sealed record AppointmentCompletedV1Data(Guid AppointmentId, Guid AgentId, Guid TenantId, string? Notes);
