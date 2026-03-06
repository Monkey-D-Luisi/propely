// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.Application.Appointments.Interfaces;

public interface ICalendarConnectionRepository
{
    Task<CalendarConnection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CalendarConnection?> GetByAgentAndProviderAsync(Guid agentId, Guid tenantId, CalendarProvider provider, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CalendarConnection>> GetActiveByAgentAsync(Guid agentId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CalendarConnection>> ListActiveAsync(CancellationToken cancellationToken = default);
    Task AddAsync(CalendarConnection connection, CancellationToken cancellationToken = default);
    void Update(CalendarConnection connection);
}
