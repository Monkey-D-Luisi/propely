// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.ContactsApi.Application.Common.Models;
using Propely.ContactsApi.Application.Contacts.Dtos;
using Propely.ContactsApi.Application.Contacts.Interfaces;

namespace Propely.ContactsApi.Application.Contacts.Queries.ListContacts;

public sealed class ListContactsQueryHandler : IRequestHandler<ListContactsQuery, PagedResult<ContactListItemDto>>
{
    private readonly IContactReadRepository _contactReadRepository;

    public ListContactsQueryHandler(IContactReadRepository contactReadRepository)
    {
        _contactReadRepository = contactReadRepository;
    }

    public async Task<PagedResult<ContactListItemDto>> Handle(ListContactsQuery request, CancellationToken cancellationToken)
    {
        var filter = new ContactListFilter
        {
            TenantId = request.TenantId,
            Search = request.Search,
            Role = request.Role,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            Page = request.Page,
            PageSize = request.PageSize
        };

        var result = await _contactReadRepository.ListAsync(filter, cancellationToken);

        return new PagedResult<ContactListItemDto>(
            result.Items.Select(ContactMapper.ToListItemDto).ToList(),
            result.PageNumber,
            result.TotalPages,
            result.TotalCount);
    }
}
