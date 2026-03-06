// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.ContactsApi.Application.Common.Models;
using Propely.ContactsApi.Application.Contacts.Dtos;
using Propely.ContactsApi.Domain.Contacts;

namespace Propely.ContactsApi.Application.Contacts.Queries.ListContacts;

public sealed record ListContactsQuery : IRequest<PagedResult<ContactListItemDto>>
{
    public Guid TenantId { get; init; }
    public string? Search { get; init; }
    public ContactRole? Role { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
