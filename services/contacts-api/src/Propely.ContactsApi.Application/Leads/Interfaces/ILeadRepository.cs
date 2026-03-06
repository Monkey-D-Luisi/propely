// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Domain.Leads;

namespace Propely.ContactsApi.Application.Leads.Interfaces;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(Lead lead, CancellationToken cancellationToken = default);
    void Update(Lead lead);
    void Delete(Lead lead);
}
