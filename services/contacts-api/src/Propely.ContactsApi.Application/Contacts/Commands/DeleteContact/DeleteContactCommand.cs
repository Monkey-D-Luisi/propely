// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.ContactsApi.Application.Contacts.Commands.DeleteContact;

public sealed record DeleteContactCommand(Guid ContactId, Guid TenantId) : IRequest;
