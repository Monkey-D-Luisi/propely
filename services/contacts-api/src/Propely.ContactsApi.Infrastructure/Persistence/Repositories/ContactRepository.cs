// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Application.Contacts.Interfaces;
using Propely.ContactsApi.Domain.Contacts;
using Microsoft.EntityFrameworkCore;

namespace Propely.ContactsApi.Infrastructure.Persistence.Repositories;

public sealed class ContactRepository : IContactRepository
{
    private readonly AppDbContext _context;

    public ContactRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Contact?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Contacts
            .Include(c => c.PropertyInterests)
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId, cancellationToken);
    }

    public async Task AddAsync(Contact contact, CancellationToken cancellationToken = default)
    {
        await _context.Contacts.AddAsync(contact, cancellationToken);
    }

    public void Update(Contact contact)
    {
        _context.Contacts.Update(contact);
    }

    public void Delete(Contact contact)
    {
        _context.Contacts.Update(contact);
    }
}
