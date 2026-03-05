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
        functionDescription: "Generate marketing copy or description text for a property listing.",
        functionParameters: BinaryData.FromString("""
        {
            "type": "object",
            "properties": {
                "property_id": { "type": "string", "description": "ID of the property to generate copy for" },
                "language": { "type": "string", "enum": ["es", "en", "fr", "de", "pt", "it"], "description": "Language for the generated copy" },
                "tone": { "type": "string", "enum": ["professional", "casual", "luxury", "family_friendly"], "description": "Tone of the generated copy" }
            },
            "required": []
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
        ReserveProperty,
        CloseOperation,
        ArchiveProperty
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
        ["reserve_property"] = ActionType.ReserveProperty,
        ["close_operation"] = ActionType.CloseOperation,
        ["archive_property"] = ActionType.ArchiveProperty
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
