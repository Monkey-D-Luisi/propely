// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Propely.AiApi.Application.Actions.Interfaces;
using Propely.AiApi.Application.Actions.Parameters;

namespace Propely.AiApi.Infrastructure.AI;

/// <summary>
/// Converts untyped action parameter dictionaries into strongly-typed parameter records
/// using System.Text.Json serialization with snake_case naming convention.
/// </summary>
public sealed class DefaultParameterBinder : IParameterBinder
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <inheritdoc/>
    public T Bind<T>(Dictionary<string, object?> raw) where T : IActionParameters
    {
        // Normalize values: convert .NET native types and JsonElement to a clean JSON-friendly dictionary,
        // then serialize to JSON and deserialize to the target type.
        var normalized = new Dictionary<string, object?>(raw.Count);

        foreach (var (key, value) in raw)
        {
            normalized[key] = NormalizeValue(value);
        }

        var json = JsonSerializer.Serialize(normalized, SerializerOptions);
        return JsonSerializer.Deserialize<T>(json, SerializerOptions)
            ?? throw new InvalidOperationException($"Failed to bind parameters to {typeof(T).Name}.");
    }

    /// <summary>
    /// Normalizes a parameter value for consistent JSON serialization.
    /// Handles JsonElement, native .NET types, and collections.
    /// </summary>
    private static object? NormalizeValue(object? value)
    {
        if (value is null)
            return null;

        if (value is JsonElement jsonElement)
            return NormalizeJsonElement(jsonElement);

        // Convert numeric types to their most compatible JSON representation
        if (value is long longValue)
            return longValue;
        if (value is double doubleValue)
            return doubleValue;
        if (value is decimal decimalValue)
            return decimalValue;
        if (value is int intValue)
            return intValue;
        if (value is Guid guidValue)
            return guidValue.ToString();
        if (value is DateTime dateTimeValue)
            return dateTimeValue.ToString("O", CultureInfo.InvariantCulture);

        // Collections
        if (value is List<string> stringList)
            return stringList;
        if (value is string[] stringArray)
            return stringArray.ToList();
        if (value is IEnumerable<object?> enumerable)
            return enumerable.Select(item => item?.ToString()).Where(s => s != null).ToList();

        return value;
    }

    /// <summary>
    /// Converts a JsonElement to a native .NET object for clean serialization.
    /// </summary>
    private static object? NormalizeJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Array => element.EnumerateArray()
                .Select(e => NormalizeJsonElement(e))
                .ToList(),
            JsonValueKind.Object => element.EnumerateObject()
                .ToDictionary(p => p.Name, p => NormalizeJsonElement(p.Value)),
            _ => element.GetRawText()
        };
    }
}
