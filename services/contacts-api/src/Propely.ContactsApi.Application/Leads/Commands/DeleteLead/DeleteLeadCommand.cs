// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.ContactsApi.Application.Leads.Commands.DeleteLead;

public sealed record DeleteLeadCommand(Guid LeadId, Guid TenantId) : IRequest;
