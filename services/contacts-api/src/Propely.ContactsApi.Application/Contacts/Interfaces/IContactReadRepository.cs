// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Application.Common.Models;
using Propely.ContactsApi.Domain.Contacts;

namespace Propely.ContactsApi.Application.Contacts.Interfaces;

public interface IContactReadRepository
{
    Task<Contact?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
    Task<Contact?> GetByEmailAsync(string email, Guid tenantId, CancellationToken cancellationToken = default);
    Task<PagedResult<Contact>> ListAsync(ContactListFilter filter, CancellationToken cancellationToken = default);
}

public sealed class ContactListFilter
{
    public Guid TenantId { get; init; }
    public string? Search { get; init; }
    public ContactRole? Role { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
