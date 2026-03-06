// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Domain.Common;

namespace Propely.AppointmentsApi.Domain.Appointments.Events;

public sealed record AppointmentCancelledV1 : IDomainEvent
{
    public Guid EventId { get; }
    public string EventType => nameof(AppointmentCancelledV1);
    public int SchemaVersion => 1;
    public DateTime OccurredAtUtc { get; }
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
    public string Producer => "AppointmentsApi";

    public AppointmentCancelledV1Data Data { get; }

    public AppointmentCancelledV1(Guid appointmentId, Guid agentId, Guid tenantId, string reason, DateTime occurredAtUtc)
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = occurredAtUtc;
        Data = new AppointmentCancelledV1Data(appointmentId, agentId, tenantId, reason);
    }
}

public sealed record AppointmentCancelledV1Data(Guid AppointmentId, Guid AgentId, Guid TenantId, string Reason);
