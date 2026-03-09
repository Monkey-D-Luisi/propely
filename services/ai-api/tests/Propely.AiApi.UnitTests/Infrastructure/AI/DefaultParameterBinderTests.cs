// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Text.Json;
using FluentAssertions;
using Propely.AiApi.Application.Actions.Parameters;
using Propely.AiApi.Infrastructure.AI;

namespace Propely.AiApi.UnitTests.Infrastructure.AI;

public sealed class DefaultParameterBinderTests
{
    private readonly DefaultParameterBinder _binder = new();

    // --- String binding ---

    [Fact]
    public void Bind_ShouldMapSnakeCaseStringToProperty()
    {
        var raw = new Dictionary<string, object?> { ["property_type"] = "apartment" };

        var result = _binder.Bind<CreatePropertyParameters>(raw);

        result.PropertyType.Should().Be("apartment");
    }

    [Fact]
    public void Bind_ShouldMapJsonElementStringToProperty()
    {
        var doc = JsonDocument.Parse("""{"property_type":"villa"}""");
        var raw = new Dictionary<string, object?>();
        foreach (var prop in doc.RootElement.EnumerateObject())
            raw[prop.Name] = prop.Value;

        var result = _binder.Bind<CreatePropertyParameters>(raw);

        result.PropertyType.Should().Be("villa");
    }

    [Fact]
    public void Bind_ShouldReturnNullForMissingStringProperty()
    {
        var raw = new Dictionary<string, object?>();

        var result = _binder.Bind<CreatePropertyParameters>(raw);

        result.PropertyType.Should().BeNull();
        result.City.Should().BeNull();
    }

    // --- Int binding ---

    [Fact]
    public void Bind_ShouldMapNativeIntToProperty()
    {
        var raw = new Dictionary<string, object?> { ["bedrooms"] = 3 };

        var result = _binder.Bind<CreatePropertyParameters>(raw);

        result.Bedrooms.Should().Be(3);
    }

    [Fact]
    public void Bind_ShouldMapJsonElementNumberToIntProperty()
    {
        var doc = JsonDocument.Parse("""{"bedrooms":4}""");
        var raw = new Dictionary<string, object?>();
        foreach (var prop in doc.RootElement.EnumerateObject())
            raw[prop.Name] = prop.Value;

        var result = _binder.Bind<CreatePropertyParameters>(raw);

        result.Bedrooms.Should().Be(4);
    }

    [Fact]
    public void Bind_ShouldReturnNullForMissingIntProperty()
    {
        var raw = new Dictionary<string, object?>();

        var result = _binder.Bind<CreatePropertyParameters>(raw);

        result.Bedrooms.Should().BeNull();
    }

    // --- Decimal binding ---

    [Fact]
    public void Bind_ShouldMapNativeDecimalToProperty()
    {
        var raw = new Dictionary<string, object?> { ["price"] = 250000m };

        var result = _binder.Bind<CreatePropertyParameters>(raw);

        result.Price.Should().Be(250000m);
    }

    [Fact]
    public void Bind_ShouldMapJsonElementNumberToDecimalProperty()
    {
        var doc = JsonDocument.Parse("""{"min_price":100000}""");
        var raw = new Dictionary<string, object?>();
        foreach (var prop in doc.RootElement.EnumerateObject())
            raw[prop.Name] = prop.Value;

        var result = _binder.Bind<QueryPropertiesParameters>(raw);

        result.MinPrice.Should().Be(100000m);
    }

    // --- Guid binding ---

    [Fact]
    public void Bind_ShouldMapStringGuidToGuidProperty()
    {
        var id = Guid.NewGuid();
        var raw = new Dictionary<string, object?> { ["property_id"] = id.ToString() };

        var result = _binder.Bind<UpdatePropertyParameters>(raw);

        result.PropertyId.Should().Be(id);
    }

    [Fact]
    public void Bind_ShouldMapJsonElementStringToGuidProperty()
    {
        var id = Guid.NewGuid();
        var doc = JsonDocument.Parse($$$"""{"property_id":"{{{id}}}"}""");
        var raw = new Dictionary<string, object?>();
        foreach (var prop in doc.RootElement.EnumerateObject())
            raw[prop.Name] = prop.Value;

        var result = _binder.Bind<UpdatePropertyParameters>(raw);

        result.PropertyId.Should().Be(id);
    }

    [Fact]
    public void Bind_ShouldReturnNullForMissingGuidProperty()
    {
        var raw = new Dictionary<string, object?>();

        var result = _binder.Bind<UpdatePropertyParameters>(raw);

        result.PropertyId.Should().BeNull();
    }

    // --- DateTime binding ---

    [Fact]
    public void Bind_ShouldMapStringDateTimeToDateTimeProperty()
    {
        var raw = new Dictionary<string, object?> { ["start_time"] = "2026-03-15T10:00:00Z" };

        var result = _binder.Bind<BookViewingParameters>(raw);

        result.StartTime.Should().Be(new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Bind_ShouldMapJsonElementStringToDateTimeProperty()
    {
        var doc = JsonDocument.Parse("""{"start_time":"2026-06-01T14:30:00Z"}""");
        var raw = new Dictionary<string, object?>();
        foreach (var prop in doc.RootElement.EnumerateObject())
            raw[prop.Name] = prop.Value;

        var result = _binder.Bind<BookViewingParameters>(raw);

        result.StartTime.Should().Be(new DateTime(2026, 6, 1, 14, 30, 0, DateTimeKind.Utc));
    }

    // --- List<string> binding ---

    [Fact]
    public void Bind_ShouldMapNativeStringListToListProperty()
    {
        var raw = new Dictionary<string, object?> { ["languages"] = new List<string> { "es", "en" } };

        var result = _binder.Bind<GenerateCopyParameters>(raw);

        result.Languages.Should().BeEquivalentTo(["es", "en"]);
    }

    [Fact]
    public void Bind_ShouldMapJsonElementArrayToListProperty()
    {
        var doc = JsonDocument.Parse("""{"languages":["es","fr","de"]}""");
        var raw = new Dictionary<string, object?>();
        foreach (var prop in doc.RootElement.EnumerateObject())
            raw[prop.Name] = prop.Value;

        var result = _binder.Bind<GenerateCopyParameters>(raw);

        result.Languages.Should().BeEquivalentTo(["es", "fr", "de"]);
    }

    [Fact]
    public void Bind_ShouldMapJsonElementArrayToImageUrlsProperty()
    {
        var doc = JsonDocument.Parse("""{"image_urls":["https://example.com/a.jpg","https://example.com/b.jpg"]}""");
        var raw = new Dictionary<string, object?>();
        foreach (var prop in doc.RootElement.EnumerateObject())
            raw[prop.Name] = prop.Value;

        var result = _binder.Bind<ExtractFromPhotosParameters>(raw);

        result.ImageUrls.Should().BeEquivalentTo(["https://example.com/a.jpg", "https://example.com/b.jpg"]);
    }

    // --- Full record binding (round-trip) ---

    [Fact]
    public void Bind_ShouldMapAllCreatePropertyParameters()
    {
        var raw = new Dictionary<string, object?>
        {
            ["title"] = "Beautiful Villa",
            ["property_type"] = "villa",
            ["operation_type"] = "Sale",
            ["bedrooms"] = 4,
            ["bathrooms"] = 3,
            ["area_m2"] = 200m,
            ["price"] = 500000m,
            ["city"] = "Marbella",
            ["description"] = "Stunning villa with sea views"
        };

        var result = _binder.Bind<CreatePropertyParameters>(raw);

        result.Title.Should().Be("Beautiful Villa");
        result.PropertyType.Should().Be("villa");
        result.OperationType.Should().Be("Sale");
        result.Bedrooms.Should().Be(4);
        result.Bathrooms.Should().Be(3);
        result.AreaM2.Should().Be(200m);
        result.Price.Should().Be(500000m);
        result.City.Should().Be("Marbella");
        result.Description.Should().Be("Stunning villa with sea views");
    }

    [Fact]
    public void Bind_ShouldMapAllBookViewingParameters()
    {
        var propertyId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var raw = new Dictionary<string, object?>
        {
            ["property_id"] = propertyId.ToString(),
            ["contact_id"] = contactId.ToString(),
            ["start_time"] = "2026-04-01T09:00:00Z",
            ["end_time"] = "2026-04-01T10:00:00Z",
            ["title"] = "Villa Viewing",
            ["location"] = "Calle Mayor 5",
            ["notes"] = "Client prefers morning"
        };

        var result = _binder.Bind<BookViewingParameters>(raw);

        result.PropertyId.Should().Be(propertyId);
        result.ContactId.Should().Be(contactId);
        result.StartTime.Should().Be(new DateTime(2026, 4, 1, 9, 0, 0, DateTimeKind.Utc));
        result.EndTime.Should().Be(new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc));
        result.Title.Should().Be("Villa Viewing");
        result.Location.Should().Be("Calle Mayor 5");
        result.Notes.Should().Be("Client prefers morning");
    }

    [Fact]
    public void Bind_ShouldMapAllCreateContactParameters()
    {
        var raw = new Dictionary<string, object?>
        {
            ["first_name"] = "Maria",
            ["last_name"] = "Garcia",
            ["email"] = "maria@example.com",
            ["phone"] = "+34650123456",
            ["role"] = "Buyer",
            ["company"] = "Garcia Investments",
            ["notes"] = "High-end buyer",
            ["source"] = "Referral"
        };

        var result = _binder.Bind<CreateContactParameters>(raw);

        result.FirstName.Should().Be("Maria");
        result.LastName.Should().Be("Garcia");
        result.Email.Should().Be("maria@example.com");
        result.Phone.Should().Be("+34650123456");
        result.Role.Should().Be("Buyer");
        result.Company.Should().Be("Garcia Investments");
        result.Notes.Should().Be("High-end buyer");
        result.Source.Should().Be("Referral");
    }

    // --- Edge cases ---

    [Fact]
    public void Bind_ShouldHandleEmptyDictionary()
    {
        var raw = new Dictionary<string, object?>();

        var result = _binder.Bind<CreatePropertyParameters>(raw);

        result.Should().NotBeNull();
        result.PropertyType.Should().BeNull();
        result.Bedrooms.Should().BeNull();
        result.Price.Should().BeNull();
    }

    [Fact]
    public void Bind_ShouldIgnoreUnknownKeys()
    {
        var raw = new Dictionary<string, object?>
        {
            ["property_type"] = "apartment",
            ["unknown_field"] = "should be ignored",
            ["another_extra"] = 42
        };

        var result = _binder.Bind<CreatePropertyParameters>(raw);

        result.PropertyType.Should().Be("apartment");
    }

    [Fact]
    public void Bind_ShouldHandleNullValuesGracefully()
    {
        var raw = new Dictionary<string, object?>
        {
            ["property_type"] = null,
            ["bedrooms"] = null,
            ["price"] = null
        };

        var result = _binder.Bind<CreatePropertyParameters>(raw);

        result.PropertyType.Should().BeNull();
        result.Bedrooms.Should().BeNull();
        result.Price.Should().BeNull();
    }

    [Fact]
    public void Bind_ShouldHandleLongToInt()
    {
        var raw = new Dictionary<string, object?> { ["bedrooms"] = 3L };

        var result = _binder.Bind<CreatePropertyParameters>(raw);

        result.Bedrooms.Should().Be(3);
    }

    [Fact]
    public void Bind_ShouldHandleDoubleToDecimal()
    {
        var raw = new Dictionary<string, object?> { ["price"] = 250000.50 };

        var result = _binder.Bind<CreatePropertyParameters>(raw);

        result.Price.Should().Be(250000.50m);
    }
}
