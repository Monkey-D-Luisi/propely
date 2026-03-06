// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Application.Common.Models;
using Propely.ContactsApi.Domain.Leads;

namespace Propely.ContactsApi.Application.Leads.Interfaces;

public interface ILeadReadRepository
{
    Task<Lead?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
    Task<PagedResult<Lead>> ListAsync(LeadListFilter filter, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAndPropertyAsync(string email, Guid propertyId, Guid tenantId, CancellationToken cancellationToken = default);
}

public sealed class LeadListFilter
{
    public Guid TenantId { get; init; }
    public LeadStatus? Status { get; init; }
    public Guid? PropertyId { get; init; }
    public Guid? AssignedAgentId { get; init; }
    public string? Search { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
