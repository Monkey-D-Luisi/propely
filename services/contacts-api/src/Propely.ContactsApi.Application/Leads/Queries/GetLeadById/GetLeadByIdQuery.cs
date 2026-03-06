// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.ContactsApi.Application.Leads.Dtos;

namespace Propely.ContactsApi.Application.Leads.Queries.GetLeadById;

public sealed record GetLeadByIdQuery(Guid LeadId, Guid TenantId) : IRequest<LeadDto?>;
