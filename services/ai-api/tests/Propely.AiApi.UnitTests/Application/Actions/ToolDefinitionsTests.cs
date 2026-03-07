// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using Propely.AiApi.Domain.Actions;
using Propely.AiApi.Infrastructure.AI;

namespace Propely.AiApi.UnitTests.Application.Actions;

public sealed class ToolDefinitionsTests
{
    [Fact]
    public void All_ShouldContainExpectedNumberOfTools()
    {
        // Act
        var tools = ToolDefinitions.All;

        // Assert
        tools.Should().HaveCount(16);
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
    public void ResolveActionType_WithKnownFunctionName_ShouldReturnCorrectActionType(
        string functionName, ActionType expectedType)
    {
        // Act
        var result = ToolDefinitions.ResolveActionType(functionName);

        // Assert
        result.Should().Be(expectedType);
    }

    [Theory]
    [InlineData("unknown_function")]
    [InlineData("")]
    [InlineData("nonexistent")]
    public void ResolveActionType_WithUnknownFunctionName_ShouldReturnUnknown(string functionName)
    {
        // Act
        var result = ToolDefinitions.ResolveActionType(functionName);

        // Assert
        result.Should().Be(ActionType.Unknown);
    }

    [Fact]
    public void CreateProperty_ToolShouldHaveCorrectFunctionName()
    {
        // Assert
        ToolDefinitions.CreateProperty.FunctionName.Should().Be("create_property");
    }

    [Fact]
    public void QueryProperties_ToolShouldHaveCorrectFunctionName()
    {
        // Assert
        ToolDefinitions.QueryProperties.FunctionName.Should().Be("query_properties");
    }

    [Fact]
    public void All_ToolsShouldHaveUniqueNames()
    {
        // Act
        var names = ToolDefinitions.All.Select(t => t.FunctionName).ToList();

        // Assert
        names.Should().OnlyHaveUniqueItems();
    }
}
