// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.ContactsApi.Application.Leads.Dtos;

namespace Propely.ContactsApi.Application.Leads.Commands.AssignLead;

public sealed record AssignLeadCommand(Guid LeadId, Guid TenantId, Guid AgentId) : IRequest<LeadDto>;
