// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Domain.Contacts;

namespace Propely.ContactsApi.Application.Contacts.Interfaces;

public interface IContactRepository
{
    Task<Contact?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
    Task AddAsync(Contact contact, CancellationToken cancellationToken = default);
    void Update(Contact contact);
    void Delete(Contact contact);
}
