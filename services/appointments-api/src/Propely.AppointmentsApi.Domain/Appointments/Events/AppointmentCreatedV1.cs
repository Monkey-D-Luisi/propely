// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Domain.Common;

namespace Propely.AppointmentsApi.Domain.Appointments.Events;

public sealed record AppointmentCreatedV1 : IDomainEvent
{
    public Guid EventId { get; }
    public string EventType => nameof(AppointmentCreatedV1);
    public int SchemaVersion => 1;
    public DateTime OccurredAtUtc { get; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Producer => "AppointmentsApi";

    public AppointmentCreatedV1Data Data { get; }

    public AppointmentCreatedV1(Guid appointmentId, Guid agentId, Guid tenantId, AppointmentType type, DateTime occurredAtUtc)
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = occurredAtUtc;
        Data = new AppointmentCreatedV1Data(appointmentId, agentId, tenantId, type);
    }
}

public sealed record AppointmentCreatedV1Data(Guid AppointmentId, Guid AgentId, Guid TenantId, AppointmentType Type);
