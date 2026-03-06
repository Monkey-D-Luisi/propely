// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.ContactsApi.Application.Contacts.Dtos;

namespace Propely.ContactsApi.Application.Contacts.Queries.GetContactById;

public sealed record GetContactByIdQuery(Guid ContactId, Guid TenantId) : IRequest<ContactDto?>;
