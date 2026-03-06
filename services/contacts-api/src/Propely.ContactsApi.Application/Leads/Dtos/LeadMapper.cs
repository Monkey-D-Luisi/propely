// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Domain.Leads;

namespace Propely.ContactsApi.Application.Leads.Dtos;

public static class LeadMapper
{
    public static LeadDto ToDto(Lead lead) => new()
    {
        Id = lead.Id,
        Name = lead.Name,
        Email = lead.Email,
        Phone = lead.Phone,
        Message = lead.Message,
        Source = lead.Source,
        PropertyId = lead.PropertyId,
        TenantId = lead.TenantId,
        Status = lead.Status,
        AssignedAgentId = lead.AssignedAgentId,
        ContactId = lead.ContactId,
        CreatedAtUtc = lead.CreatedAtUtc,
        UpdatedAtUtc = lead.UpdatedAtUtc
    };

    public static LeadListItemDto ToListItemDto(Lead lead) => new()
    {
        Id = lead.Id,
        Name = lead.Name,
        Email = lead.Email,
        Source = lead.Source,
        PropertyId = lead.PropertyId,
        Status = lead.Status,
        AssignedAgentId = lead.AssignedAgentId,
        CreatedAtUtc = lead.CreatedAtUtc
    };
}
