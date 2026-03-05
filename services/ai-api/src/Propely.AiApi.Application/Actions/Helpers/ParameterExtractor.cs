// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Globalization;
using System.Text.Json;

namespace Propely.AiApi.Application.Actions.Helpers;

/// <summary>
/// Static helper to safely extract typed values from action parameter dictionaries.
/// Handles both native .NET types and <see cref="JsonElement"/> values from JSON deserialization.
/// </summary>
public static class ParameterExtractor
{
    /// <summary>
    /// Extracts a string value from the parameters dictionary.
    /// </summary>
    public static string? GetString(Dictionary<string, object?> parameters, string key)
    {
        if (!parameters.TryGetValue(key, out var value) || value is null)
            return null;

        if (value is JsonElement jsonElement)
        {
            return jsonElement.ValueKind == JsonValueKind.String
                ? jsonElement.GetString()
                : jsonElement.GetRawText().Trim('"');
        }

        return value.ToString();
    }

    /// <summary>
    /// Extracts an integer value from the parameters dictionary.
    /// </summary>
    public static int? GetInt(Dictionary<string, object?> parameters, string key)
    {
        if (!parameters.TryGetValue(key, out var value) || value is null)
            return null;

        if (value is JsonElement jsonElement)
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.Number => jsonElement.TryGetInt32(out var i) ? i : null,
                JsonValueKind.String => int.TryParse(jsonElement.GetString(), CultureInfo.InvariantCulture, out var parsed) ? parsed : null,
                _ => null
            };
        }

        if (value is int intValue) return intValue;
        if (value is long longValue) return (int)longValue;
        if (value is double doubleValue) return (int)doubleValue;

        return int.TryParse(value.ToString(), CultureInfo.InvariantCulture, out var result) ? result : null;
    }

    /// <summary>
    /// Extracts a decimal value from the parameters dictionary.
    /// </summary>
    public static decimal? GetDecimal(Dictionary<string, object?> parameters, string key)
    {
        if (!parameters.TryGetValue(key, out var value) || value is null)
            return null;

        if (value is JsonElement jsonElement)
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.Number => jsonElement.TryGetDecimal(out var d) ? d : null,
                JsonValueKind.String => decimal.TryParse(jsonElement.GetString(), CultureInfo.InvariantCulture, out var parsed) ? parsed : null,
                _ => null
            };
        }

        if (value is decimal decimalValue) return decimalValue;
        if (value is double doubleValue) return (decimal)doubleValue;
        if (value is int intValue) return intValue;
        if (value is long longValue) return longValue;

        return decimal.TryParse(value.ToString(), CultureInfo.InvariantCulture, out var result) ? result : null;
    }

    /// <summary>
    /// Extracts a GUID value from the parameters dictionary.
    /// </summary>
    public static Guid? GetGuid(Dictionary<string, object?> parameters, string key)
    {
        if (!parameters.TryGetValue(key, out var value) || value is null)
            return null;

        if (value is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.String)
            {
                return Guid.TryParse(jsonElement.GetString(), out var parsed) ? parsed : null;
            }
            return null;
        }

        if (value is Guid guidValue) return guidValue;

        return Guid.TryParse(value.ToString(), out var result) ? result : null;
    }

    /// <summary>
    /// Extracts an enum value from the parameters dictionary.
    /// </summary>
    public static T? GetEnum<T>(Dictionary<string, object?> parameters, string key) where T : struct, Enum
    {
        var stringValue = GetString(parameters, key);
        if (string.IsNullOrWhiteSpace(stringValue))
            return null;

        return Enum.TryParse<T>(stringValue, ignoreCase: true, out var result) ? result : null;
    }
}
