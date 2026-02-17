// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Globalization;
using System.Text;
using System.Text.Json;
using SaasTemplate.OrgsApi.Api.Configuration;
using SaasTemplate.OrgsApi.Api.Extensions;
using SaasTemplate.OrgsApi.Application.AuditLogs.DTOs;
using SaasTemplate.OrgsApi.Application.AuditLogs.Queries.ExportAuditLogs;
using SaasTemplate.OrgsApi.Application.AuditLogs.Queries.GetAuditLogs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaasTemplate.OrgsApi.Api.Controllers;

[ApiController]
[Route("admin/audit-logs")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public sealed class AuditLogsController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonExportOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    private readonly IMediator _mediator;

    public AuditLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? action = null,
        [FromQuery] string? entityType = null,
        [FromQuery] string? entityId = null,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = this.GetUserId();
        if (currentUserId is null) return Unauthorized();

        var query = new GetAuditLogsQuery(page, pageSize, dateFrom, dateTo, userId, action, entityType, entityId);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportAuditLogs(
        [FromQuery] string format = "json",
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? action = null,
        [FromQuery] string? entityType = null,
        [FromQuery] string? entityId = null,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = this.GetUserId();
        if (currentUserId is null) return Unauthorized();

        var query = new ExportAuditLogsQuery(dateFrom, dateTo, userId, action, entityType, entityId);
        var items = await _mediator.Send(query, cancellationToken);

        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = BuildCsv(items);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "audit-logs.csv");
        }

        var json = JsonSerializer.Serialize(items, JsonExportOptions);
        return File(Encoding.UTF8.GetBytes(json), "application/json", "audit-logs.json");
    }

    private static string BuildCsv(List<AuditLogDto> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,UserId,OrganizationId,Action,EntityType,EntityId,Changes,CorrelationId,CreatedAtUtc");

        foreach (var item in items)
        {
            sb.AppendLine(string.Join(",",
                item.Id,
                item.UserId?.ToString() ?? "",
                item.OrganizationId?.ToString() ?? "",
                EscapeCsv(item.Action),
                EscapeCsv(item.EntityType),
                EscapeCsv(item.EntityId),
                EscapeCsv(item.Changes ?? ""),
                EscapeCsv(item.CorrelationId ?? ""),
                item.CreatedAtUtc.ToString("o", CultureInfo.InvariantCulture)));
        }

        return sb.ToString();
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
