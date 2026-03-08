// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentAssertions;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Propely.AiApi.IntegrationTests.Fixtures;

namespace Propely.AiApi.IntegrationTests.MCP;

/// <summary>
/// Integration tests for the MCP Streamable HTTP endpoint.
/// Uses the official MCP .NET client to verify tools/list and tools/call.
/// </summary>
public sealed class McpEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public McpEndpointTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task McpEndpoint_ListTools_ReturnsAll20Tools()
    {
        // Arrange — create MCP client using the test server's HttpClient
        await using var client = await CreateMcpClientAsync();

        // Act
        var tools = await client.ListToolsAsync();

        // Assert
        tools.Should().HaveCount(20, "all 20 Propely AI action tools should be exposed via MCP");

        var toolNames = tools.Select(t => t.Name).ToList();
        toolNames.Should().Contain("create_property");
        toolNames.Should().Contain("query_properties");
        toolNames.Should().Contain("update_property");
        toolNames.Should().Contain("change_property_status");
        toolNames.Should().Contain("generate_copy");
        toolNames.Should().Contain("extract_from_text");
        toolNames.Should().Contain("extract_from_photos");
        toolNames.Should().Contain("create_lead");
        toolNames.Should().Contain("create_contact");
        toolNames.Should().Contain("qualify_lead");
        toolNames.Should().Contain("convert_lead");
        toolNames.Should().Contain("query_leads");
        toolNames.Should().Contain("book_viewing");
        toolNames.Should().Contain("query_appointments");
        toolNames.Should().Contain("cancel_appointment");
        toolNames.Should().Contain("reschedule_appointment");
        toolNames.Should().Contain("reserve_property");
        toolNames.Should().Contain("close_operation");
        toolNames.Should().Contain("archive_property");
        toolNames.Should().Contain("reactivate_property");
    }

    [Fact]
    public async Task McpEndpoint_ToolsCallCreateProperty_ReturnsResult()
    {
        // Arrange
        await using var client = await CreateMcpClientAsync();

        // Act
        var result = await client.CallToolAsync(
            "create_property",
            new Dictionary<string, object?>
            {
                ["title"] = "MCP Integration Test Property",
                ["city"] = "Madrid",
                ["price"] = 250000,
                ["bedrooms"] = 3,
                ["property_type"] = "apartment",
                ["operation_type"] = "sale"
            });

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().NotBeEmpty();
        var textContent = result.Content.OfType<TextContentBlock>().FirstOrDefault();
        textContent.Should().NotBeNull();
        textContent!.Text.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task McpEndpoint_ToolsCallUnknownTool_ThrowsMcpProtocolException()
    {
        // Arrange
        await using var client = await CreateMcpClientAsync();

        // Act & Assert — MCP SDK validates tool names and throws McpProtocolException
        var act = async () => await client.CallToolAsync(
            "nonexistent_tool",
            new Dictionary<string, object?>());

        await act.Should().ThrowAsync<McpProtocolException>()
            .WithMessage("*Unknown tool*");
    }

    [Fact]
    public async Task McpEndpoint_ToolSchemas_HaveDescriptions()
    {
        // Arrange
        await using var client = await CreateMcpClientAsync();

        // Act
        var tools = await client.ListToolsAsync();

        // Assert — every tool should have a description
        foreach (var tool in tools)
        {
            tool.Description.Should().NotBeNullOrEmpty(
                $"tool '{tool.Name}' should have a description");
        }
    }

    private async Task<McpClient> CreateMcpClientAsync()
    {
        var httpClient = _factory.CreateClient();

        var transport = new HttpClientTransport(
            new HttpClientTransportOptions
            {
                Endpoint = new Uri(httpClient.BaseAddress!, "/mcp"),
                TransportMode = HttpTransportMode.StreamableHttp
            },
            httpClient);

        return await McpClient.CreateAsync(transport);
    }
}
