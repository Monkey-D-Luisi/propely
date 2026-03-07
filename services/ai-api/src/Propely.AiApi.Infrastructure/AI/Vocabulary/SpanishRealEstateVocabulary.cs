// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Globalization;

namespace Propely.AiApi.Infrastructure.AI.Vocabulary;

/// <summary>
/// Bidirectional mapping of Spanish real estate property type terms to normalized enum values.
/// Handles accented/unaccented variants, plural forms, and common synonyms.
/// </summary>
public static class SpanishRealEstateVocabulary
{
    private static readonly Dictionary<string, string> PropertyTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Apartment variants
        ["piso"] = "apartment",
        ["pisos"] = "apartment",
        ["apartamento"] = "apartment",
        ["apartamentos"] = "apartment",
        ["apartment"] = "apartment",

        // Penthouse variants
        ["atico"] = "penthouse",
        ["ático"] = "penthouse",
        ["aticos"] = "penthouse",
        ["áticos"] = "penthouse",
        ["penthouse"] = "penthouse",

        // Ground floor
        ["bajo"] = "apartment",
        ["bajos"] = "apartment",

        // Duplex
        ["duplex"] = "duplex",
        ["dúplex"] = "duplex",

        // Loft / Studio
        ["loft"] = "studio",
        ["lofts"] = "studio",
        ["estudio"] = "studio",
        ["estudios"] = "studio",
        ["studio"] = "studio",

        // Townhouse / Semi-detached
        ["adosado"] = "house",
        ["adosados"] = "house",
        ["pareado"] = "house",
        ["pareados"] = "house",

        // House / Villa
        ["chalet"] = "house",
        ["chalets"] = "house",
        ["chalé"] = "house",
        ["casa"] = "house",
        ["casas"] = "house",
        ["house"] = "house",

        // Villa / Country estate
        ["villa"] = "villa",
        ["villas"] = "villa",
        ["finca"] = "villa",
        ["fincas"] = "villa",
        ["cortijo"] = "villa",
        ["cortijos"] = "villa",
        ["masía"] = "villa",
        ["masia"] = "villa",
        ["masias"] = "villa",
        ["masías"] = "villa",

        // Commercial
        ["local"] = "commercial",
        ["local comercial"] = "commercial",
        ["locales"] = "commercial",
        ["locales comerciales"] = "commercial",
        ["oficina"] = "commercial",
        ["oficinas"] = "commercial",
        ["nave"] = "commercial",
        ["nave industrial"] = "commercial",
        ["naves"] = "commercial",
        ["naves industriales"] = "commercial",
        ["commercial"] = "commercial",

        // Land
        ["solar"] = "land",
        ["solares"] = "land",
        ["terreno"] = "land",
        ["terrenos"] = "land",
        ["parcela"] = "land",
        ["parcelas"] = "land",
        ["land"] = "land",

        // Garage
        ["garaje"] = "garage",
        ["garajes"] = "garage",
        ["plaza de garaje"] = "garage",
        ["garage"] = "garage",

        // Storage
        ["trastero"] = "storage",
        ["trasteros"] = "storage",
        ["storage"] = "storage",

        // Edificio (building)
        ["edificio"] = "commercial",
        ["edificios"] = "commercial"
    };

    private static readonly Dictionary<string, string> OperationTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Sale
        ["venta"] = "sale",
        ["ventas"] = "sale",
        ["vender"] = "sale",
        ["comprar"] = "sale",
        ["compra"] = "sale",
        ["sale"] = "sale",

        // Rent
        ["alquiler"] = "rent",
        ["alquilar"] = "rent",
        ["arrendar"] = "rent",
        ["arriendo"] = "rent",
        ["rent"] = "rent",

        // Vacation rental
        ["alquiler vacacional"] = "rent",
        ["alquiler temporal"] = "rent",

        // Transfer
        ["traspaso"] = "transfer",
        ["traspasar"] = "transfer",
        ["transfer"] = "transfer",

        // Rent-to-own
        ["alquiler con opcion a compra"] = "rent",
        ["alquiler con opción a compra"] = "rent"
    };

    private static readonly Dictionary<string, string> FeatureMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["piscina"] = "pool",
        ["jardín"] = "garden",
        ["jardin"] = "garden",
        ["garaje"] = "garage",
        ["trastero"] = "storage",
        ["aire acondicionado"] = "air_conditioning",
        ["calefacción"] = "heating",
        ["calefaccion"] = "heating",
        ["ascensor"] = "elevator",
        ["terraza"] = "terrace",
        ["balcón"] = "balcony",
        ["balcon"] = "balcony",
        ["amueblado"] = "furnished",
        ["luminoso"] = "bright",
        ["exterior"] = "exterior",
        ["interior"] = "interior",
        ["reformado"] = "renovated",
        ["a estrenar"] = "new_build"
    };

    /// <summary>
    /// Resolves a Spanish (or English) property type term to its normalized enum value.
    /// Returns null if the term is not recognized.
    /// </summary>
    public static string? ResolvePropertyType(string term)
    {
        var normalized = NormalizeInput(term);
        return PropertyTypeMap.TryGetValue(normalized, out var result) ? result : null;
    }

    /// <summary>
    /// Resolves a Spanish (or English) operation type term to its normalized enum value.
    /// Returns null if the term is not recognized.
    /// </summary>
    public static string? ResolveOperationType(string term)
    {
        var normalized = NormalizeInput(term);
        return OperationTypeMap.TryGetValue(normalized, out var result) ? result : null;
    }

    /// <summary>
    /// Resolves a Spanish feature term to its normalized value.
    /// Returns null if the term is not recognized.
    /// </summary>
    public static string? ResolveFeature(string term)
    {
        var normalized = NormalizeInput(term);
        return FeatureMap.TryGetValue(normalized, out var result) ? result : null;
    }

    /// <summary>
    /// Returns all known Spanish property type terms for use in prompt context.
    /// </summary>
    public static IReadOnlyDictionary<string, string> GetPropertyTypeMappings() => PropertyTypeMap;

    /// <summary>
    /// Returns all known Spanish operation type terms for use in prompt context.
    /// </summary>
    public static IReadOnlyDictionary<string, string> GetOperationTypeMappings() => OperationTypeMap;

    /// <summary>
    /// Returns all known Spanish feature terms for use in prompt context.
    /// </summary>
    public static IReadOnlyDictionary<string, string> GetFeatureMappings() => FeatureMap;

    private static string NormalizeInput(string input)
        => input.Trim().ToLower(CultureInfo.InvariantCulture);
}
