// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.ContactsApi.Application.Leads.Dtos;
using Propely.ContactsApi.Domain.Leads;

namespace Propely.ContactsApi.Application.Leads.Commands.ChangeLeadStatus;

public sealed record ChangeLeadStatusCommand(Guid LeadId, Guid TenantId, LeadStatus NewStatus) : IRequest<LeadDto>;
