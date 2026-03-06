// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Domain.Appointments;

namespace Propely.AppointmentsApi.Application.Appointments.Interfaces;

public interface ISyncOperationRepository
{
    Task<IReadOnlyList<SyncOperation>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task AddAsync(SyncOperation operation, CancellationToken cancellationToken = default);
    void Update(SyncOperation operation);
}
