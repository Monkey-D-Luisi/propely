// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.Json;
using OpenAI.Chat;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.Infrastructure.AI;

/// <summary>
/// Static definitions for OpenAI function calling tools.
/// Each tool represents an action the AI can classify user input into.
/// Uses snake_case parameter naming per OpenAI convention.
/// </summary>
public static class ToolDefinitions
{
    // --- Property Actions ---

    public static readonly ChatTool CreateProperty = ChatTool.CreateFunctionTool(
        functionName: "create_property",
        functionDescription: "Create a new real estate property listing with the provided details.",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "title": { "type": "string", "description": "Title or name of the property listing" },
                "property_type": { "type": "string", "enum": ["apartment", "house", "villa", "studio", "penthouse", "duplex", "commercial", "land", "garage", "storage"], "description": "Type of property" },
                "operation_type": { "type": "string", "enum": ["sale", "rent", "transfer"], "description": "Whether the property is for sale, rent, or transfer" },
                "bedrooms": { "type": "integer", "description": "Number of bedrooms" },
                "bathrooms": { "type": "integer", "description": "Number of bathrooms" },
                "price": { "type": "number", "description": "Price in EUR" },
                "city": { "type": "string", "description": "City where the property is located" },
                "description": { "type": "string", "description": "Detailed description of the property" }
            },
            "required": []
        }
        """));

    public static readonly ChatTool UpdateProperty = ChatTool.CreateFunctionTool(
        functionName: "update_property",
        functionDescription: "Update an existing property listing by modifying one or more fields.",
        functionParameters: BinaryData.FromString("""
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
        """));

    public static readonly ChatTool QueryProperties = ChatTool.CreateFunctionTool(
        functionName: "query_properties",
        functionDescription: "Search and filter properties based on various criteria like type, location, price range, and status.",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "property_type": { "type": "string", "enum": ["apartment", "house", "villa", "studio", "penthouse", "duplex", "commercial", "land", "garage", "storage"], "description": "Filter by property type" },
                "operation_type": { "type": "string", "enum": ["sale", "rent", "transfer"], "description": "Filter by operation type" },
                "city": { "type": "string", "description": "Filter by city" },
                "min_price": { "type": "number", "description": "Minimum price in EUR" },
                "max_price": { "type": "number", "description": "Maximum price in EUR" },
                "min_bedrooms": { "type": "integer", "description": "Minimum number of bedrooms" },
                "status": { "type": "string", "enum": ["draft", "active", "reserved", "sold", "rented", "archived"], "description": "Filter by property status" }
            },
            "required": []
        }
        """));

    public static readonly ChatTool ChangePropertyStatus = ChatTool.CreateFunctionTool(
        functionName: "change_property_status",
        functionDescription: "Change the status of a property (e.g., activate, deactivate, mark as sold).",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "property_id": { "type": "string", "description": "ID of the property" },
                "status": { "type": "string", "enum": ["draft", "active", "reserved", "sold", "rented", "archived"], "description": "New status for the property" },
                "reference": { "type": "string", "description": "Property reference code or name to identify it" }
            },
            "required": []
        }
        """));

    // --- Content Generation Actions ---

    public static readonly ChatTool GenerateCopy = ChatTool.CreateFunctionTool(
        functionName: "generate_copy",
        functionDescription: "Generate marketing copy or description text for a property listing in multiple languages.",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "property_data": { "type": "string", "description": "Description of the property to generate copy for (type, location, size, features, price)" },
                "tone": { "type": "string", "enum": ["professional", "luxury", "casual", "concise"], "description": "Tone of the generated copy" },
                "languages": { "type": "array", "items": { "type": "string", "enum": ["es", "en", "fr", "de", "nl"] }, "description": "Languages to generate copy in. Defaults to all 5 if not specified." }
            },
            "required": ["property_data"]
        }
        """));

    public static readonly ChatTool ExtractFromText = ChatTool.CreateFunctionTool(
        functionName: "extract_from_text",
        functionDescription: "Extract structured property data from unstructured text such as emails, descriptions, or notes.",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "text": { "type": "string", "description": "The unstructured text to extract data from" }
            },
            "required": ["text"]
        }
        """));

    public static readonly ChatTool ExtractFromPhotos = ChatTool.CreateFunctionTool(
        functionName: "extract_from_photos",
        functionDescription: "Extract structured property data from photographs using AI vision analysis. Identifies property type, features, room count, and condition.",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "image_urls": { "type": "array", "items": { "type": "string" }, "description": "Array of image URLs to analyze (max 10)" }
            },
            "required": ["image_urls"]
        }
        """));

    // --- Contact & Lead Actions ---

    public static readonly ChatTool CreateLead = ChatTool.CreateFunctionTool(
        functionName: "create_lead",
        functionDescription: "Create a new lead (potential client inquiry) for a specific property.",
        functionParameters: BinaryData.FromString("""
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
        """));

    public static readonly ChatTool CreateContact = ChatTool.CreateFunctionTool(
        functionName: "create_contact",
        functionDescription: "Create a new contact (buyer, seller, tenant, landlord, or professional).",
        functionParameters: BinaryData.FromString("""
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
        """));

    public static readonly ChatTool QualifyLead = ChatTool.CreateFunctionTool(
        functionName: "qualify_lead",
        functionDescription: "Qualify a lead, changing its status to Qualified to indicate it has been vetted.",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "lead_id": { "type": "string", "description": "ID of the lead to qualify" }
            },
            "required": ["lead_id"]
        }
        """));

    public static readonly ChatTool ConvertLead = ChatTool.CreateFunctionTool(
        functionName: "convert_lead",
        functionDescription: "Convert a qualified lead into a full contact record.",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "lead_id": { "type": "string", "description": "ID of the lead to convert" },
                "role": { "type": "string", "enum": ["Buyer", "Seller", "Tenant", "Landlord", "Professional"], "description": "Role to assign to the new contact. Defaults to Buyer." },
                "notes": { "type": "string", "description": "Notes for the conversion" }
            },
            "required": ["lead_id"]
        }
        """));

    public static readonly ChatTool QueryLeads = ChatTool.CreateFunctionTool(
        functionName: "query_leads",
        functionDescription: "Search and filter leads based on status, property, or name.",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "status": { "type": "string", "enum": ["New", "Contacted", "Qualified", "Converted", "Lost"], "description": "Filter by lead status" },
                "property_id": { "type": "string", "description": "Filter by property ID" },
                "search": { "type": "string", "description": "Search by lead name or email" }
            },
            "required": []
        }
        """));

    // --- Operation Actions ---

    public static readonly ChatTool ReserveProperty = ChatTool.CreateFunctionTool(
        functionName: "reserve_property",
        functionDescription: "Reserve a property for a potential buyer or renter.",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "property_id": { "type": "string", "description": "ID of the property to reserve" },
                "contact_name": { "type": "string", "description": "Name of the person reserving the property" },
                "reference": { "type": "string", "description": "Property reference code or name to identify it" }
            },
            "required": []
        }
        """));

    public static readonly ChatTool CloseOperation = ChatTool.CreateFunctionTool(
        functionName: "close_operation",
        functionDescription: "Close a real estate operation (sale or rental) on a property.",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "property_id": { "type": "string", "description": "ID of the property" },
                "reference": { "type": "string", "description": "Property reference code or name to identify it" }
            },
            "required": []
        }
        """));

    public static readonly ChatTool ArchiveProperty = ChatTool.CreateFunctionTool(
        functionName: "archive_property",
        functionDescription: "Archive a property listing, removing it from active listings.",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "property_id": { "type": "string", "description": "ID of the property to archive" },
                "reference": { "type": "string", "description": "Property reference code or name to identify it" }
            },
            "required": []
        }
        """));

    public static readonly ChatTool ReactivateProperty = ChatTool.CreateFunctionTool(
        functionName: "reactivate_property",
        functionDescription: "Reactivate an archived or withdrawn property listing, returning it to active status.",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "property_id": { "type": "string", "description": "ID of the property to reactivate" },
                "reference": { "type": "string", "description": "Property reference code or name to identify it" }
            },
            "required": []
        }
        """));

    /// <summary>
    /// Returns all available tool definitions for intent classification.
    /// </summary>
    public static IReadOnlyList<ChatTool> All =>
    [
        CreateProperty,
        UpdateProperty,
        QueryProperties,
        ChangePropertyStatus,
        GenerateCopy,
        ExtractFromText,
        ExtractFromPhotos,
        CreateLead,
        CreateContact,
        QualifyLead,
        ConvertLead,
        QueryLeads,
        ReserveProperty,
        CloseOperation,
        ArchiveProperty,
        ReactivateProperty
    ];

    /// <summary>
    /// Maps OpenAI function names to ActionType enum values.
    /// </summary>
    private static readonly Dictionary<string, ActionType> FunctionNameToActionType = new()
    {
        ["create_property"] = ActionType.CreateProperty,
        ["update_property"] = ActionType.UpdateProperty,
        ["query_properties"] = ActionType.QueryProperties,
        ["change_property_status"] = ActionType.ChangePropertyStatus,
        ["generate_copy"] = ActionType.GenerateCopy,
        ["extract_from_text"] = ActionType.ExtractFromText,
        ["extract_from_photos"] = ActionType.ExtractFromPhotos,
        ["create_lead"] = ActionType.CreateLead,
        ["create_contact"] = ActionType.CreateContact,
        ["qualify_lead"] = ActionType.QualifyLead,
        ["convert_lead"] = ActionType.ConvertLead,
        ["query_leads"] = ActionType.QueryLeads,
        ["reserve_property"] = ActionType.ReserveProperty,
        ["close_operation"] = ActionType.CloseOperation,
        ["archive_property"] = ActionType.ArchiveProperty,
        ["reactivate_property"] = ActionType.ReactivateProperty
    };

    /// <summary>
    /// Resolves an OpenAI function name to the corresponding ActionType.
    /// Returns ActionType.Unknown if the function name is not recognized.
    /// </summary>
    public static ActionType ResolveActionType(string functionName)
        => FunctionNameToActionType.TryGetValue(functionName, out var actionType)
            ? actionType
            : ActionType.Unknown;
}
