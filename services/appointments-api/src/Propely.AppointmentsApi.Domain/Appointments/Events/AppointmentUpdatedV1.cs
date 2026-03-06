// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Domain.Common;

namespace Propely.AppointmentsApi.Domain.Appointments.Events;

public sealed record AppointmentUpdatedV1 : IDomainEvent
{
    public Guid EventId { get; }
    public string EventType => nameof(AppointmentUpdatedV1);
    public int SchemaVersion => 1;
    public DateTime OccurredAtUtc { get; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Producer => "AppointmentsApi";

    public AppointmentUpdatedV1Data Data { get; }

    public AppointmentUpdatedV1(Guid appointmentId, Guid agentId, Guid tenantId, DateTime occurredAtUtc)
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = occurredAtUtc;
        Data = new AppointmentUpdatedV1Data(appointmentId, agentId, tenantId);
    }
}

public sealed record AppointmentUpdatedV1Data(Guid AppointmentId, Guid AgentId, Guid TenantId);
