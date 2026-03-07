// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Application.Actions.Tools;

/// <summary>
/// In-memory registry of all 20 provider-neutral tool schemas.
/// The JSON Schema strings are identical to the original ToolDefinitions
/// to ensure behavioral equivalence.
/// </summary>
public sealed class ToolSchemaRegistry : IToolSchemaRegistry
{
    private readonly Dictionary<string, ToolSchema> _byName;

    public ToolSchemaRegistry()
    {
        var schemas = CreateSchemas();
        _byName = schemas.ToDictionary(s => s.Name);
        All = schemas;
    }

    public IReadOnlyList<ToolSchema> All { get; }

    public ToolSchema? GetByName(string name)
        => _byName.TryGetValue(name, out var schema) ? schema : null;

    public ActionType ResolveActionType(string functionName)
        => _byName.TryGetValue(functionName, out var schema) ? schema.ActionType : ActionType.Unknown;

    private static List<ToolSchema> CreateSchemas() =>
    [
        // --- Property Actions ---

        new ToolSchema(
            Name: "create_property",
            Description: "Create a new real estate property listing with the provided details.",
            ParametersJsonSchema: """
            {
                "type": "object",
                "properties": {
                    "title": { "type": "string", "description": "Title or name of the property listing" },
                    "property_type": { "type": "string", "enum": ["apartment", "house", "villa", "studio", "penthouse", "duplex", "commercial", "land", "garage", "storage"], "description": "Type of property (piso=apartment, chalet/casa=house, ático=penthouse, estudio/loft=studio, finca/cortijo=villa, local/oficina/nave=commercial, solar/terreno=land, garaje=garage, trastero=storage)" },
                    "operation_type": { "type": "string", "enum": ["sale", "rent", "transfer"], "description": "Whether the property is for sale, rent, or transfer (venta/compra=sale, alquiler=rent, traspaso=transfer)" },
                    "bedrooms": { "type": "integer", "description": "Number of bedrooms (habitaciones, dormitorios)" },
                    "bathrooms": { "type": "integer", "description": "Number of bathrooms" },
                    "price": { "type": "number", "description": "Price in EUR" },
                    "city": { "type": "string", "description": "City where the property is located" },
                    "description": { "type": "string", "description": "Detailed description of the property" }
                },
                "required": []
            }
            """,
            ActionType: ActionType.CreateProperty),

        new ToolSchema(
            Name: "update_property",
            Description: "Update an existing property listing by modifying one or more fields.",
            ParametersJsonSchema: """
            {
                "type": "object",
                "properties": {
                    "property_id": { "type": "string", "description": "ID of the property to update" },
                    "title": { "type": "string", "description": "New title for the property" },
                    "price": { "type": "number", "description": "New price in EUR" },
                    "description": { "type": "string", "description": "New description" },
                    "field": { "type": "string", "description": "Specific field name to update" },
                    "value": { "type": "string", "description": "New value for the specified field" }
                },
                "required": []
            }
            """,
            ActionType: ActionType.UpdateProperty),

        new ToolSchema(
            Name: "query_properties",
            Description: "Search and filter properties based on various criteria like type, location, price range, and status.",
            ParametersJsonSchema: """
            {
                "type": "object",
                "properties": {
                    "property_type": { "type": "string", "enum": ["apartment", "house", "villa", "studio", "penthouse", "duplex", "commercial", "land", "garage", "storage"], "description": "Filter by property type (piso=apartment, chalet/casa=house, ático=penthouse, estudio/loft=studio, finca/cortijo=villa, local/oficina/nave=commercial, solar/terreno=land)" },
                    "operation_type": { "type": "string", "enum": ["sale", "rent", "transfer"], "description": "Filter by operation type (venta/compra=sale, alquiler=rent, traspaso=transfer)" },
                    "city": { "type": "string", "description": "Filter by city" },
                    "min_price": { "type": "number", "description": "Minimum price in EUR" },
                    "max_price": { "type": "number", "description": "Maximum price in EUR" },
                    "min_bedrooms": { "type": "integer", "description": "Minimum number of bedrooms" },
                    "status": { "type": "string", "enum": ["draft", "active", "reserved", "sold", "rented", "archived"], "description": "Filter by property status" }
                },
                "required": []
            }
            """,
            ActionType: ActionType.QueryProperties),

        new ToolSchema(
            Name: "change_property_status",
            Description: "Change the status of a property (e.g., activate, deactivate, mark as sold).",
            ParametersJsonSchema: """
            {
                "type": "object",
                "properties": {
                    "property_id": { "type": "string", "description": "ID of the property" },
                    "status": { "type": "string", "enum": ["draft", "active", "reserved", "sold", "rented", "archived"], "description": "New status for the property" },
                    "reference": { "type": "string", "description": "Property reference code or name to identify it" }
                },
                "required": []
            }
            """,
            ActionType: ActionType.ChangePropertyStatus),

        // --- Content Generation Actions ---

        new ToolSchema(
            Name: "generate_copy",
            Description: "Generate marketing copy or description text for a property listing in multiple languages.",
            ParametersJsonSchema: """
            {
                "type": "object",
                "properties": {
                    "property_data": { "type": "string", "description": "Description of the property to generate copy for (type, location, size, features, price)" },
                    "tone": { "type": "string", "enum": ["professional", "luxury", "casual", "concise"], "description": "Tone of the generated copy" },
                    "languages": { "type": "array", "items": { "type": "string", "enum": ["es", "en", "fr", "de", "nl"] }, "description": "Languages to generate copy in. Defaults to all 5 if not specified." }
                },
                "required": ["property_data"]
            }
            """,
            ActionType: ActionType.GenerateCopy),

        new ToolSchema(
            Name: "extract_from_text",
            Description: "Extract structured property data from unstructured text such as emails, descriptions, or notes.",
            ParametersJsonSchema: """
            {
                "type": "object",
                "properties": {
                    "text": { "type": "string", "description": "The unstructured text to extract data from" }
                },
                "required": ["text"]
            }
            """,
            ActionType: ActionType.ExtractFromText),

        new ToolSchema(
            Name: "extract_from_photos",
            Description: "Extract structured property data from photographs using AI vision analysis. Identifies property type, features, room count, and condition.",
            ParametersJsonSchema: """
            {
                "type": "object",
                "properties": {
                    "image_urls": { "type": "array", "items": { "type": "string" }, "description": "Array of image URLs to analyze (max 10)" }
                },
                "required": ["image_urls"]
            }
            """,
            ActionType: ActionType.ExtractFromPhotos),

        // --- Contact & Lead Actions ---

        new ToolSchema(
            Name: "create_lead",
            Description: "Create a new lead (potential client inquiry) for a specific property.",
            ParametersJsonSchema: """
            {
                "type": "object",
                "properties": {
                    "name": { "type": "string", "description": "Full name of the lead (e.g., Maria Garcia)" },
                    "email": { "type": "string", "description": "Email address of the lead" },
                    "phone": { "type": "string", "description": "Phone number of the lead" },
                    "property_id": { "type": "string", "description": "ID of the property the lead is interested in" },
                    "message": { "type": "string", "description": "The lead's inquiry message or notes" },
                    "source": { "type": "string", "description": "Source of the lead (e.g., Portal, Phone, WalkIn, Website)" }
                },
                "required": ["name", "email", "property_id"]
            }
            """,
            ActionType: ActionType.CreateLead),

        new ToolSchema(
            Name: "create_contact",
            Description: "Create a new contact (buyer, seller, tenant, landlord, or professional).",
            ParametersJsonSchema: """
            {
                "type": "object",
                "properties": {
                    "first_name": { "type": "string", "description": "First name of the contact" },
                    "last_name": { "type": "string", "description": "Last name of the contact" },
                    "email": { "type": "string", "description": "Email address of the contact" },
                    "phone": { "type": "string", "description": "Phone number of the contact" },
                    "role": { "type": "string", "enum": ["buyer", "seller", "tenant", "landlord", "professional"], "description": "Role of the contact (comprador, vendedor, inquilino, propietario, profesional)" },
                    "company": { "type": "string", "description": "Company name if applicable" },
                    "notes": { "type": "string", "description": "Notes about the contact" },
                    "source": { "type": "string", "description": "Source of the contact (e.g., Portal, Phone, WalkIn, Website, Referral)" }
                },
                "required": ["first_name", "last_name", "email"]
            }
            """,
            ActionType: ActionType.CreateContact),

        new ToolSchema(
            Name: "qualify_lead",
            Description: "Qualify a lead, changing its status to Qualified to indicate it has been vetted.",
            ParametersJsonSchema: """
            {
                "type": "object",
                "properties": {
                    "lead_id": { "type": "string", "description": "ID of the lead to qualify" }
                },
                "required": ["lead_id"]
            }
            """,
            ActionType: ActionType.QualifyLead),

        new ToolSchema(
            Name: "convert_lead",
            Description: "Convert a qualified lead into a full contact record.",
            ParametersJsonSchema: """
            {
                "type": "object",
                "properties": {
                    "lead_id": { "type": "string", "description": "ID of the lead to convert" },
                    "role": { "type": "string", "enum": ["Buyer", "Seller", "Tenant", "Landlord", "Professional"], "description": "Role to assign to the new contact. Defaults to Buyer." },
                    "notes": { "type": "string", "description": "Notes for the conversion" }
                },
                "required": ["lead_id"]
            }
            """,
            ActionType: ActionType.ConvertLead),

        new ToolSchema(
            Name: "query_leads",
            Description: "Search and filter leads based on status, property, or name.",
            ParametersJsonSchema: """
            {
                "type": "object",
                "properties": {
                    "status": { "type": "string", "enum": ["New", "Contacted", "Qualified", "Converted", "Lost"], "description": "Filter by lead status" },
                    "property_id": { "type": "string", "description": "Filter by property ID" },
                    "search": { "type": "string", "description": "Search by lead name or email" }
                },
                "required": []
            }
            """,
            ActionType: ActionType.QueryLeads),

        // --- Appointment Actions ---

        new ToolSchema(
            Name: "book_viewing",
            Description: "Book a property viewing appointment for a contact at a specific date and time.",
            ParametersJsonSchema: """
            {
                "type": "object",
                "properties": {
                    "property_id": { "type": "string", "description": "ID of the property to view" },
                    "contact_id": { "type": "string", "description": "ID of the contact attending the viewing" },
                    "start_time": { "type": "string", "description": "Start date and time in ISO 8601 format (e.g., 2026-03-15T10:00:00)" },
                    "end_time": { "type": "string", "description": "Optional end time. Defaults to 30 minutes after start." },
                    "title": { "type": "string", "description": "Optional title for the appointment" },
                    "location": { "type": "string", "description": "Location or address of the viewing" },
                    "notes": { "type": "string", "description": "Notes for the viewing" }
                },
                "required": ["property_id", "start_time"]
            }
            """,
            ActionType: ActionType.BookViewing),

        new ToolSchema(
            Name: "query_appointments",
            Description: "Search and list appointments filtered by date range, status, property, or type.",
            ParametersJsonSchema: """
            {
                "type": "object",
                "properties": {
                    "status": { "type": "string", "enum": ["Scheduled", "Confirmed", "Completed", "Cancelled", "NoShow"], "description": "Filter by appointment status" },
                    "type": { "type": "string", "enum": ["PropertyViewing", "OwnerMeeting", "Generic"], "description": "Filter by appointment type" },
                    "property_id": { "type": "string", "description": "Filter by property ID" },
                    "from_date": { "type": "string", "description": "Start of date range in ISO 8601 format" },
                    "to_date": { "type": "string", "description": "End of date range in ISO 8601 format" }
                },
                "required": []
            }
            """,
            ActionType: ActionType.QueryAppointments),

        new ToolSchema(
            Name: "cancel_appointment",
            Description: "Cancel a scheduled appointment with an optional reason.",
            ParametersJsonSchema: """
            {
                "type": "object",
                "properties": {
                    "appointment_id": { "type": "string", "description": "ID of the appointment to cancel" },
                    "reason": { "type": "string", "description": "Reason for cancelling" }
                },
                "required": ["appointment_id"]
            }
            """,
            ActionType: ActionType.CancelAppointment),

        new ToolSchema(
            Name: "reschedule_appointment",
            Description: "Reschedule an existing appointment to a new date and time.",
            ParametersJsonSchema: """
            {
                "type": "object",
                "properties": {
                    "appointment_id": { "type": "string", "description": "ID of the appointment to reschedule" },
                    "new_start_time": { "type": "string", "description": "New start date and time in ISO 8601 format" },
                    "new_end_time": { "type": "string", "description": "Optional new end time" }
                },
                "required": ["appointment_id", "new_start_time"]
            }
            """,
            ActionType: ActionType.RescheduleAppointment),

        // --- Operation Actions ---

        new ToolSchema(
            Name: "reserve_property",
            Description: "Reserve a property for a potential buyer or renter.",
            ParametersJsonSchema: """
            {
                "type": "object",
                "properties": {
                    "property_id": { "type": "string", "description": "ID of the property to reserve" },
                    "contact_name": { "type": "string", "description": "Name of the person reserving the property" },
                    "reference": { "type": "string", "description": "Property reference code or name to identify it" }
                },
                "required": []
            }
            """,
            ActionType: ActionType.ReserveProperty),

        new ToolSchema(
            Name: "close_operation",
            Description: "Close a real estate operation (sale or rental) on a property.",
            ParametersJsonSchema: """
            {
                "type": "object",
                "properties": {
                    "property_id": { "type": "string", "description": "ID of the property" },
                    "reference": { "type": "string", "description": "Property reference code or name to identify it" }
                },
                "required": []
            }
            """,
            ActionType: ActionType.CloseOperation),

        new ToolSchema(
            Name: "archive_property",
            Description: "Archive a property listing, removing it from active listings.",
            ParametersJsonSchema: """
            {
                "type": "object",
                "properties": {
                    "property_id": { "type": "string", "description": "ID of the property to archive" },
                    "reference": { "type": "string", "description": "Property reference code or name to identify it" }
                },
                "required": []
            }
            """,
            ActionType: ActionType.ArchiveProperty),

        new ToolSchema(
            Name: "reactivate_property",
            Description: "Reactivate an archived or withdrawn property listing, returning it to active status.",
            ParametersJsonSchema: """
            {
                "type": "object",
                "properties": {
                    "property_id": { "type": "string", "description": "ID of the property to reactivate" },
                    "reference": { "type": "string", "description": "Property reference code or name to identify it" }
                },
                "required": []
            }
            """,
            ActionType: ActionType.ReactivateProperty),
    ];
}
