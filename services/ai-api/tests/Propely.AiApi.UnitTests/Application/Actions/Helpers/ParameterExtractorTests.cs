// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.Json;
using FluentAssertions;
using Propely.AiApi.Application.Actions.Helpers;

namespace Propely.AiApi.UnitTests.Application.Actions.Helpers;

public sealed class ParameterExtractorTests
{
    // --- GetString ---

    [Fact]
    public void GetString_WhenKeyExists_ShouldReturnStringValue()
    {
        var parameters = new Dictionary<string, object?> { ["city"] = "Malaga" };

        var result = ParameterExtractor.GetString(parameters, "city");

        result.Should().Be("Malaga");
    }

    [Fact]
    public void GetString_WhenKeyMissing_ShouldReturnNull()
    {
        var parameters = new Dictionary<string, object?>();

        var result = ParameterExtractor.GetString(parameters, "city");

        result.Should().BeNull();
    }

    [Fact]
    public void GetString_WhenValueIsNull_ShouldReturnNull()
    {
        var parameters = new Dictionary<string, object?> { ["city"] = null };

        var result = ParameterExtractor.GetString(parameters, "city");

        result.Should().BeNull();
    }

    [Fact]
    public void GetString_WhenValueIsJsonElement_ShouldReturnString()
    {
        var json = JsonSerializer.Deserialize<JsonElement>("""{"city": "Barcelona"}""");
        var cityElement = json.GetProperty("city");
        var parameters = new Dictionary<string, object?> { ["city"] = cityElement };

        var result = ParameterExtractor.GetString(parameters, "city");

        result.Should().Be("Barcelona");
    }

    [Fact]
    public void GetString_WhenValueIsIntegerType_ShouldReturnStringRepresentation()
    {
        var parameters = new Dictionary<string, object?> { ["count"] = 42 };

        var result = ParameterExtractor.GetString(parameters, "count");

        result.Should().Be("42");
    }

    // --- GetInt ---

    [Fact]
    public void GetInt_WhenValueIsInt_ShouldReturnIntValue()
    {
        var parameters = new Dictionary<string, object?> { ["bedrooms"] = 3 };

        var result = ParameterExtractor.GetInt(parameters, "bedrooms");

        result.Should().Be(3);
    }

    [Fact]
    public void GetInt_WhenValueIsLong_ShouldReturnIntValue()
    {
        var parameters = new Dictionary<string, object?> { ["bedrooms"] = 3L };

        var result = ParameterExtractor.GetInt(parameters, "bedrooms");

        result.Should().Be(3);
    }

    [Fact]
    public void GetInt_WhenValueIsDouble_ShouldReturnIntValue()
    {
        var parameters = new Dictionary<string, object?> { ["bedrooms"] = 3.0 };

        var result = ParameterExtractor.GetInt(parameters, "bedrooms");

        result.Should().Be(3);
    }

    [Fact]
    public void GetInt_WhenValueIsString_ShouldParseAndReturn()
    {
        var parameters = new Dictionary<string, object?> { ["bedrooms"] = "4" };

        var result = ParameterExtractor.GetInt(parameters, "bedrooms");

        result.Should().Be(4);
    }

    [Fact]
    public void GetInt_WhenKeyMissing_ShouldReturnNull()
    {
        var parameters = new Dictionary<string, object?>();

        var result = ParameterExtractor.GetInt(parameters, "bedrooms");

        result.Should().BeNull();
    }

    [Fact]
    public void GetInt_WhenValueIsJsonElementNumber_ShouldReturnInt()
    {
        var json = JsonSerializer.Deserialize<JsonElement>("""{"bedrooms": 5}""");
        var element = json.GetProperty("bedrooms");
        var parameters = new Dictionary<string, object?> { ["bedrooms"] = element };

        var result = ParameterExtractor.GetInt(parameters, "bedrooms");

        result.Should().Be(5);
    }

    [Fact]
    public void GetInt_WhenValueIsJsonElementString_ShouldParseAndReturn()
    {
        var json = JsonSerializer.Deserialize<JsonElement>("""{"bedrooms": "6"}""");
        var element = json.GetProperty("bedrooms");
        var parameters = new Dictionary<string, object?> { ["bedrooms"] = element };

        var result = ParameterExtractor.GetInt(parameters, "bedrooms");

        result.Should().Be(6);
    }

    [Fact]
    public void GetInt_WhenValueIsUnparsableString_ShouldReturnNull()
    {
        var parameters = new Dictionary<string, object?> { ["bedrooms"] = "not-a-number" };

        var result = ParameterExtractor.GetInt(parameters, "bedrooms");

        result.Should().BeNull();
    }

    // --- GetDecimal ---

    [Fact]
    public void GetDecimal_WhenValueIsDecimal_ShouldReturnDecimalValue()
    {
        var parameters = new Dictionary<string, object?> { ["price"] = 250000m };

        var result = ParameterExtractor.GetDecimal(parameters, "price");

        result.Should().Be(250000m);
    }

    [Fact]
    public void GetDecimal_WhenValueIsDouble_ShouldReturnDecimalValue()
    {
        var parameters = new Dictionary<string, object?> { ["price"] = 250000.50 };

        var result = ParameterExtractor.GetDecimal(parameters, "price");

        result.Should().Be(250000.50m);
    }

    [Fact]
    public void GetDecimal_WhenValueIsInt_ShouldReturnDecimalValue()
    {
        var parameters = new Dictionary<string, object?> { ["price"] = 250000 };

        var result = ParameterExtractor.GetDecimal(parameters, "price");

        result.Should().Be(250000m);
    }

    [Fact]
    public void GetDecimal_WhenValueIsJsonElementNumber_ShouldReturnDecimal()
    {
        var json = JsonSerializer.Deserialize<JsonElement>("""{"price": 350000.75}""");
        var element = json.GetProperty("price");
        var parameters = new Dictionary<string, object?> { ["price"] = element };

        var result = ParameterExtractor.GetDecimal(parameters, "price");

        result.Should().Be(350000.75m);
    }

    [Fact]
    public void GetDecimal_WhenKeyMissing_ShouldReturnNull()
    {
        var parameters = new Dictionary<string, object?>();

        var result = ParameterExtractor.GetDecimal(parameters, "price");

        result.Should().BeNull();
    }

    // --- GetGuid ---

    [Fact]
    public void GetGuid_WhenValueIsGuid_ShouldReturnGuidValue()
    {
        var guid = Guid.NewGuid();
        var parameters = new Dictionary<string, object?> { ["property_id"] = guid };

        var result = ParameterExtractor.GetGuid(parameters, "property_id");

        result.Should().Be(guid);
    }

    [Fact]
    public void GetGuid_WhenValueIsGuidString_ShouldParseAndReturn()
    {
        var guid = Guid.NewGuid();
        var parameters = new Dictionary<string, object?> { ["property_id"] = guid.ToString() };

        var result = ParameterExtractor.GetGuid(parameters, "property_id");

        result.Should().Be(guid);
    }

    [Fact]
    public void GetGuid_WhenValueIsJsonElementString_ShouldParseAndReturn()
    {
        var guid = Guid.NewGuid();
        var json = JsonSerializer.Deserialize<JsonElement>($$$"""{"property_id": "{{{guid}}}"}""");
        var element = json.GetProperty("property_id");
        var parameters = new Dictionary<string, object?> { ["property_id"] = element };

        var result = ParameterExtractor.GetGuid(parameters, "property_id");

        result.Should().Be(guid);
    }

    [Fact]
    public void GetGuid_WhenKeyMissing_ShouldReturnNull()
    {
        var parameters = new Dictionary<string, object?>();

        var result = ParameterExtractor.GetGuid(parameters, "property_id");

        result.Should().BeNull();
    }

    [Fact]
    public void GetGuid_WhenValueIsInvalidString_ShouldReturnNull()
    {
        var parameters = new Dictionary<string, object?> { ["property_id"] = "not-a-guid" };

        var result = ParameterExtractor.GetGuid(parameters, "property_id");

        result.Should().BeNull();
    }

    // --- GetEnum ---

    private enum TestStatus { Draft, Active, Sold }

    [Fact]
    public void GetEnum_WhenValueIsValidString_ShouldReturnEnumValue()
    {
        var parameters = new Dictionary<string, object?> { ["status"] = "Active" };

        var result = ParameterExtractor.GetEnum<TestStatus>(parameters, "status");

        result.Should().Be(TestStatus.Active);
    }

    [Fact]
    public void GetEnum_WhenValueIsCaseInsensitive_ShouldReturnEnumValue()
    {
        var parameters = new Dictionary<string, object?> { ["status"] = "active" };

        var result = ParameterExtractor.GetEnum<TestStatus>(parameters, "status");

        result.Should().Be(TestStatus.Active);
    }

    [Fact]
    public void GetEnum_WhenValueIsInvalid_ShouldReturnNull()
    {
        var parameters = new Dictionary<string, object?> { ["status"] = "InvalidValue" };

        var result = ParameterExtractor.GetEnum<TestStatus>(parameters, "status");

        result.Should().BeNull();
    }

    [Fact]
    public void GetEnum_WhenKeyMissing_ShouldReturnNull()
    {
        var parameters = new Dictionary<string, object?>();

        var result = ParameterExtractor.GetEnum<TestStatus>(parameters, "status");

        result.Should().BeNull();
    }
}
