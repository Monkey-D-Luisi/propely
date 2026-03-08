// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json;
using ModelContextProtocol.Server;
using Propely.AiApi.Domain.Actions;
using Propely.AiApi.Infrastructure.MCP;

namespace Propely.AiApi.Api.MCP;

/// <summary>
/// MCP tool definitions exposing all 20 Propely AI action tools.
/// Each method is auto-discovered by the MCP SDK via <see cref="McpServerToolTypeAttribute"/>.
/// DI services (<see cref="McpToolHandler"/>, <see cref="ClaimsPrincipal"/>) are auto-resolved
/// from the method parameters and do not appear in the tool's JSON Schema.
/// </summary>
[McpServerToolType]
public static class PropelyMcpTools
{
    // ── Property Actions ──

    [McpServerTool(Name = "create_property"), Description("Create a new real estate property listing with the provided details.")]
    public static async Task<string> CreateProperty(
        McpToolHandler handler, ClaimsPrincipal user,
        [Description("Property title")] string? title = null,
        [Description("Property type (apartment, house, villa, land, commercial, office)")] string? property_type = null,
        [Description("Operation type (sale, rent)")] string? operation_type = null,
        [Description("Number of bedrooms")] int? bedrooms = null,
        [Description("Number of bathrooms")] int? bathrooms = null,
        [Description("Price in EUR")] decimal? price = null,
        [Description("City name")] string? city = null,
        [Description("Property description")] string? description = null)
    {
        return await ExecuteToolAsync(handler, user, "create_property", new Dictionary<string, object?>
        {
            ["title"] = title, ["property_type"] = property_type, ["operation_type"] = operation_type,
            ["bedrooms"] = bedrooms, ["bathrooms"] = bathrooms, ["price"] = price,
            ["city"] = city, ["description"] = description
        });
    }

    [McpServerTool(Name = "update_property"), Description("Update an existing property listing by modifying one or more fields.")]
    public static async Task<string> UpdateProperty(
        McpToolHandler handler, ClaimsPrincipal user,
        [Description("Property ID (UUID)")] Guid? property_id = null,
        [Description("Property reference code")] string? reference = null,
        [Description("Field name to update")] string? field = null,
        [Description("New value for the field")] string? value = null)
    {
        return await ExecuteToolAsync(handler, user, "update_property", new Dictionary<string, object?>
        {
            ["property_id"] = property_id, ["reference"] = reference,
            ["field"] = field, ["value"] = value
        });
    }

    [McpServerTool(Name = "query_properties"), Description("Search and filter properties based on various criteria like type, location, price range, and status.")]
    public static async Task<string> QueryProperties(
        McpToolHandler handler, ClaimsPrincipal user,
        [Description("Property type filter")] string? property_type = null,
        [Description("Operation type filter (sale, rent)")] string? operation_type = null,
        [Description("Status filter")] string? status = null,
        [Description("City filter")] string? city = null,
        [Description("Minimum price")] decimal? min_price = null,
        [Description("Maximum price")] decimal? max_price = null,
        [Description("Minimum bedrooms")] int? min_bedrooms = null)
    {
        return await ExecuteToolAsync(handler, user, "query_properties", new Dictionary<string, object?>
        {
            ["property_type"] = property_type, ["operation_type"] = operation_type, ["status"] = status,
            ["city"] = city, ["min_price"] = min_price, ["max_price"] = max_price, ["min_bedrooms"] = min_bedrooms
        });
    }

    [McpServerTool(Name = "change_property_status"), Description("Change the status of a property (e.g., activate, deactivate, mark as sold).")]
    public static async Task<string> ChangePropertyStatus(
        McpToolHandler handler, ClaimsPrincipal user,
        [Description("Property ID (UUID)")] Guid? property_id = null,
        [Description("Property reference code")] string? reference = null,
        [Description("New status")] string? status = null)
    {
        return await ExecuteToolAsync(handler, user, "change_property_status", new Dictionary<string, object?>
        {
            ["property_id"] = property_id, ["reference"] = reference, ["status"] = status
        });
    }

    // ── Content / AI Generation Actions ──

    [McpServerTool(Name = "generate_copy"), Description("Generate marketing copy or description text for a property listing in multiple languages.")]
    public static async Task<string> GenerateCopy(
        McpToolHandler handler, ClaimsPrincipal user,
        [Description("Property data or description to generate copy from")] string? property_data = null,
        [Description("Tone of the copy (professional, casual, luxury)")] string? tone = null,
        [Description("Languages to generate copy in")] List<string>? languages = null)
    {
        return await ExecuteToolAsync(handler, user, "generate_copy", new Dictionary<string, object?>
        {
            ["property_data"] = property_data, ["tone"] = tone, ["languages"] = languages
        });
    }

    [McpServerTool(Name = "extract_from_text"), Description("Extract structured property data from unstructured text such as emails, descriptions, or notes.")]
    public static async Task<string> ExtractFromText(
        McpToolHandler handler, ClaimsPrincipal user,
        [Description("The text to extract property data from")] string? text = null)
    {
        return await ExecuteToolAsync(handler, user, "extract_from_text", new Dictionary<string, object?>
        {
            ["text"] = text
        });
    }

    [McpServerTool(Name = "extract_from_photos"), Description("Extract structured property data from photographs using AI vision analysis.")]
    public static async Task<string> ExtractFromPhotos(
        McpToolHandler handler, ClaimsPrincipal user,
        [Description("URLs of images to analyze")] List<string>? image_urls = null)
    {
        return await ExecuteToolAsync(handler, user, "extract_from_photos", new Dictionary<string, object?>
        {
            ["image_urls"] = image_urls
        });
    }

    // ── Contact & Lead Actions ──

    [McpServerTool(Name = "create_lead"), Description("Create a new lead (potential client inquiry) for a specific property.")]
    public static async Task<string> CreateLead(
        McpToolHandler handler, ClaimsPrincipal user,
        [Description("Lead name")] string? name = null,
        [Description("Email address")] string? email = null,
        [Description("Phone number")] string? phone = null,
        [Description("Lead message")] string? message = null,
        [Description("Lead source")] string? source = null,
        [Description("Property ID (UUID)")] Guid? property_id = null)
    {
        return await ExecuteToolAsync(handler, user, "create_lead", new Dictionary<string, object?>
        {
            ["name"] = name, ["email"] = email, ["phone"] = phone,
            ["message"] = message, ["source"] = source, ["property_id"] = property_id
        });
    }

    [McpServerTool(Name = "create_contact"), Description("Create a new contact (buyer, seller, tenant, landlord, or professional).")]
    public static async Task<string> CreateContact(
        McpToolHandler handler, ClaimsPrincipal user,
        [Description("First name")] string? first_name = null,
        [Description("Last name")] string? last_name = null,
        [Description("Email address")] string? email = null,
        [Description("Phone number")] string? phone = null,
        [Description("Role (buyer, seller, tenant, landlord)")] string? role = null,
        [Description("Company name")] string? company = null,
        [Description("Additional notes")] string? notes = null,
        [Description("Contact source")] string? source = null)
    {
        return await ExecuteToolAsync(handler, user, "create_contact", new Dictionary<string, object?>
        {
            ["first_name"] = first_name, ["last_name"] = last_name, ["email"] = email, ["phone"] = phone,
            ["role"] = role, ["company"] = company, ["notes"] = notes, ["source"] = source
        });
    }

    [McpServerTool(Name = "qualify_lead"), Description("Qualify a lead, changing its status to Qualified to indicate it has been vetted.")]
    public static async Task<string> QualifyLead(
        McpToolHandler handler, ClaimsPrincipal user,
        [Description("Lead ID (UUID)")] Guid? lead_id = null)
    {
        return await ExecuteToolAsync(handler, user, "qualify_lead", new Dictionary<string, object?>
        {
            ["lead_id"] = lead_id
        });
    }

    [McpServerTool(Name = "convert_lead"), Description("Convert a qualified lead into a full contact record.")]
    public static async Task<string> ConvertLead(
        McpToolHandler handler, ClaimsPrincipal user,
        [Description("Lead ID (UUID)")] Guid? lead_id = null,
        [Description("Contact role")] string? role = null,
        [Description("Conversion notes")] string? notes = null)
    {
        return await ExecuteToolAsync(handler, user, "convert_lead", new Dictionary<string, object?>
        {
            ["lead_id"] = lead_id, ["role"] = role, ["notes"] = notes
        });
    }

    [McpServerTool(Name = "query_leads"), Description("Search and filter leads based on status, property, or name.")]
    public static async Task<string> QueryLeads(
        McpToolHandler handler, ClaimsPrincipal user,
        [Description("Status filter")] string? status = null,
        [Description("Property ID filter (UUID)")] Guid? property_id = null,
        [Description("Search term")] string? search = null)
    {
        return await ExecuteToolAsync(handler, user, "query_leads", new Dictionary<string, object?>
        {
            ["status"] = status, ["property_id"] = property_id, ["search"] = search
        });
    }

    // ── Appointment Actions ──

    [McpServerTool(Name = "book_viewing"), Description("Book a property viewing appointment for a contact at a specific date and time.")]
    public static async Task<string> BookViewing(
        McpToolHandler handler, ClaimsPrincipal user,
        [Description("Property ID (UUID)")] Guid? property_id = null,
        [Description("Contact ID (UUID)")] Guid? contact_id = null,
        [Description("Start time (ISO 8601)")] DateTime? start_time = null,
        [Description("End time (ISO 8601)")] DateTime? end_time = null,
        [Description("Appointment title")] string? title = null,
        [Description("Viewing location")] string? location = null,
        [Description("Additional notes")] string? notes = null)
    {
        return await ExecuteToolAsync(handler, user, "book_viewing", new Dictionary<string, object?>
        {
            ["property_id"] = property_id, ["contact_id"] = contact_id,
            ["start_time"] = start_time, ["end_time"] = end_time,
            ["title"] = title, ["location"] = location, ["notes"] = notes
        });
    }

    [McpServerTool(Name = "query_appointments"), Description("Search and list appointments filtered by date range, status, property, or type.")]
    public static async Task<string> QueryAppointments(
        McpToolHandler handler, ClaimsPrincipal user,
        [Description("Status filter")] string? status = null,
        [Description("Appointment type filter")] string? type = null,
        [Description("Property ID filter (UUID)")] Guid? property_id = null,
        [Description("Start of date range (ISO 8601)")] DateTime? from_date = null,
        [Description("End of date range (ISO 8601)")] DateTime? to_date = null)
    {
        return await ExecuteToolAsync(handler, user, "query_appointments", new Dictionary<string, object?>
        {
            ["status"] = status, ["type"] = type, ["property_id"] = property_id,
            ["from_date"] = from_date, ["to_date"] = to_date
        });
    }

    [McpServerTool(Name = "cancel_appointment"), Description("Cancel a scheduled appointment with an optional reason.")]
    public static async Task<string> CancelAppointment(
        McpToolHandler handler, ClaimsPrincipal user,
        [Description("Appointment ID (UUID)")] Guid? appointment_id = null,
        [Description("Cancellation reason")] string? reason = null)
    {
        return await ExecuteToolAsync(handler, user, "cancel_appointment", new Dictionary<string, object?>
        {
            ["appointment_id"] = appointment_id, ["reason"] = reason
        });
    }

    [McpServerTool(Name = "reschedule_appointment"), Description("Reschedule an existing appointment to a new date and time.")]
    public static async Task<string> RescheduleAppointment(
        McpToolHandler handler, ClaimsPrincipal user,
        [Description("Appointment ID (UUID)")] Guid? appointment_id = null,
        [Description("New start time (ISO 8601)")] DateTime? new_start_time = null,
        [Description("New end time (ISO 8601)")] DateTime? new_end_time = null)
    {
        return await ExecuteToolAsync(handler, user, "reschedule_appointment", new Dictionary<string, object?>
        {
            ["appointment_id"] = appointment_id, ["new_start_time"] = new_start_time, ["new_end_time"] = new_end_time
        });
    }

    // ── Operation Actions ──

    [McpServerTool(Name = "reserve_property"), Description("Reserve a property for a potential buyer or renter.")]
    public static async Task<string> ReserveProperty(
        McpToolHandler handler, ClaimsPrincipal user,
        [Description("Property ID (UUID)")] Guid? property_id = null,
        [Description("Property reference code")] string? reference = null,
        [Description("Contact name for reservation")] string? contact_name = null)
    {
        return await ExecuteToolAsync(handler, user, "reserve_property", new Dictionary<string, object?>
        {
            ["property_id"] = property_id, ["reference"] = reference, ["contact_name"] = contact_name
        });
    }

    [McpServerTool(Name = "close_operation"), Description("Close a real estate operation (sale or rental) on a property.")]
    public static async Task<string> CloseOperation(
        McpToolHandler handler, ClaimsPrincipal user,
        [Description("Property ID (UUID)")] Guid? property_id = null,
        [Description("Property reference code")] string? reference = null,
        [Description("Operation type (sale, rent)")] string? operation_type = null)
    {
        return await ExecuteToolAsync(handler, user, "close_operation", new Dictionary<string, object?>
        {
            ["property_id"] = property_id, ["reference"] = reference, ["operation_type"] = operation_type
        });
    }

    [McpServerTool(Name = "archive_property"), Description("Archive a property listing, removing it from active listings.")]
    public static async Task<string> ArchiveProperty(
        McpToolHandler handler, ClaimsPrincipal user,
        [Description("Property ID (UUID)")] Guid? property_id = null,
        [Description("Property reference code")] string? reference = null)
    {
        return await ExecuteToolAsync(handler, user, "archive_property", new Dictionary<string, object?>
        {
            ["property_id"] = property_id, ["reference"] = reference
        });
    }

    [McpServerTool(Name = "reactivate_property"), Description("Reactivate an archived or withdrawn property listing, returning it to active status.")]
    public static async Task<string> ReactivateProperty(
        McpToolHandler handler, ClaimsPrincipal user,
        [Description("Property ID (UUID)")] Guid? property_id = null,
        [Description("Property reference code")] string? reference = null)
    {
        return await ExecuteToolAsync(handler, user, "reactivate_property", new Dictionary<string, object?>
        {
            ["property_id"] = property_id, ["reference"] = reference
        });
    }

    // ── Shared execution ──

    private static async Task<string> ExecuteToolAsync(
        McpToolHandler handler,
        ClaimsPrincipal user,
        string toolName,
        Dictionary<string, object?> arguments)
    {
        var (tenantId, agentId) = ExtractUserContext(user);

        // Strip null values — MCP clients may not send optional params
        var filtered = new Dictionary<string, object?>();
        foreach (var (key, value) in arguments)
        {
            if (value is not null)
                filtered[key] = value;
        }

        var result = await handler.HandleToolCallAsync(toolName, filtered, tenantId, agentId);

        // Return structured JSON — the MCP SDK wraps exceptions as IsError automatically
        if (!result.Success)
        {
            var errors = result.Errors.Length > 0 ? string.Join("; ", result.Errors) : "Unknown error";
            throw new InvalidOperationException(result.Message ?? errors);
        }

        return JsonSerializer.Serialize(new
        {
            success = result.Success,
            actionType = result.ActionType.ToString(),
            message = result.Message,
            data = result.Data
        }, JsonOptions);
    }

    private static (Guid tenantId, Guid agentId) ExtractUserContext(ClaimsPrincipal user)
    {
        var orgIdClaim = user.FindFirst("org_id")?.Value;
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var tenantId = Guid.TryParse(orgIdClaim, out var t) ? t : Guid.Empty;
        var agentId = Guid.TryParse(userIdClaim, out var a) ? a : Guid.Empty;

        return (tenantId, agentId);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };
}
