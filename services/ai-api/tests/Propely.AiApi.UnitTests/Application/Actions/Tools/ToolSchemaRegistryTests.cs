// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.AiApi.Application.Actions.Tools;
using Propely.AiApi.Domain.Actions;

namespace Propely.AiApi.UnitTests.Application.Actions.Tools;

public sealed class ToolSchemaRegistryTests
{
    private readonly ToolSchemaRegistry _registry = new();

    [Fact]
    public void All_ShouldContainExpectedNumberOfTools()
    {
        _registry.All.Should().HaveCount(20);
    }

    [Fact]
    public void All_ToolsShouldHaveUniqueNames()
    {
        var names = _registry.All.Select(t => t.Name).ToList();
        names.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void All_ToolsShouldHaveNonEmptyDescriptions()
    {
        foreach (var schema in _registry.All)
        {
            schema.Description.Should().NotBeNullOrWhiteSpace(
                because: $"tool '{schema.Name}' must have a description");
        }
    }

    [Fact]
    public void All_ToolsShouldHaveValidJsonSchemas()
    {
        foreach (var schema in _registry.All)
        {
            var act = () => System.Text.Json.JsonDocument.Parse(schema.ParametersJsonSchema);
            act.Should().NotThrow(
                because: $"tool '{schema.Name}' must have valid JSON Schema");
        }
    }

    [Theory]
    [InlineData("create_property", ActionType.CreateProperty)]
    [InlineData("update_property", ActionType.UpdateProperty)]
    [InlineData("query_properties", ActionType.QueryProperties)]
    [InlineData("change_property_status", ActionType.ChangePropertyStatus)]
    [InlineData("generate_copy", ActionType.GenerateCopy)]
    [InlineData("extract_from_text", ActionType.ExtractFromText)]
    [InlineData("extract_from_photos", ActionType.ExtractFromPhotos)]
    [InlineData("reserve_property", ActionType.ReserveProperty)]
    [InlineData("close_operation", ActionType.CloseOperation)]
    [InlineData("archive_property", ActionType.ArchiveProperty)]
    [InlineData("reactivate_property", ActionType.ReactivateProperty)]
    [InlineData("create_lead", ActionType.CreateLead)]
    [InlineData("create_contact", ActionType.CreateContact)]
    [InlineData("qualify_lead", ActionType.QualifyLead)]
    [InlineData("convert_lead", ActionType.ConvertLead)]
    [InlineData("query_leads", ActionType.QueryLeads)]
    [InlineData("book_viewing", ActionType.BookViewing)]
    [InlineData("query_appointments", ActionType.QueryAppointments)]
    [InlineData("cancel_appointment", ActionType.CancelAppointment)]
    [InlineData("reschedule_appointment", ActionType.RescheduleAppointment)]
    public void ResolveActionType_WithKnownFunctionName_ShouldReturnCorrectActionType(
        string functionName, ActionType expectedType)
    {
        _registry.ResolveActionType(functionName).Should().Be(expectedType);
    }

    [Theory]
    [InlineData("unknown_function")]
    [InlineData("")]
    [InlineData("nonexistent")]
    public void ResolveActionType_WithUnknownFunctionName_ShouldReturnUnknown(string functionName)
    {
        _registry.ResolveActionType(functionName).Should().Be(ActionType.Unknown);
    }

    [Fact]
    public void GetByName_WithExistingName_ShouldReturnSchema()
    {
        var schema = _registry.GetByName("create_property");
        schema.Should().NotBeNull();
        schema!.Name.Should().Be("create_property");
        schema.ActionType.Should().Be(ActionType.CreateProperty);
    }

    [Fact]
    public void GetByName_WithNonExistentName_ShouldReturnNull()
    {
        _registry.GetByName("does_not_exist").Should().BeNull();
    }

    [Fact]
    public void All_ShouldNotContainUnknownActionType()
    {
        _registry.All.Should().NotContain(s => s.ActionType == ActionType.Unknown);
    }
}
