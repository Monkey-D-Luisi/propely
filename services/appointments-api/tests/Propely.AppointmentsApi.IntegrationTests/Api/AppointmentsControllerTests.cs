// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Propely.AppointmentsApi.Application.Appointments.Dtos;
using Propely.AppointmentsApi.Application.Common.Models;
using Propely.AppointmentsApi.Domain.Appointments;
using Propely.AppointmentsApi.IntegrationTests.Fixtures;

namespace Propely.AppointmentsApi.IntegrationTests.Api;

/// <summary>
/// Integration tests for AppointmentsController CRUD endpoints.
/// </summary>
public sealed class AppointmentsControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public AppointmentsControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListAppointments_ReturnsOk_WithPagedResult()
    {
        // Act
        var response = await _client.GetAsync("/api/appointments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<AppointmentListItemDto>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task CreateAppointment_ReturnsCreated_WithValidRequest()
    {
        // Arrange
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var startTime = DateTime.UtcNow.AddHours(1);
        var endTime = startTime.AddHours(1);

        var request = new
        {
            Title = "Viewing " + uniqueSuffix,
            Type = "PropertyViewing",
            StartTimeUtc = startTime,
            EndTimeUtc = endTime,
            Location = "123 Main St",
            PropertyId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments", request, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var appointment = await response.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);
        appointment.Should().NotBeNull();
        appointment!.Id.Should().NotBeEmpty();
        appointment.Title.Should().Be(request.Title);
        appointment.Type.Should().Be(AppointmentType.PropertyViewing);
        appointment.Location.Should().Be("123 Main St");
    }

    [Fact]
    public async Task GetAppointmentById_ReturnsNotFound_WhenAppointmentDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/appointments/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAppointmentById_ReturnsAppointment_AfterCreation()
    {
        // Arrange: Create an appointment first
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var startTime = DateTime.UtcNow.AddHours(2);
        var endTime = startTime.AddHours(1);

        var createRequest = new
        {
            Title = "Retrievable Viewing " + uniqueSuffix,
            Type = "OwnerMeeting",
            StartTimeUtc = startTime,
            EndTimeUtc = endTime,
            Description = "Test description"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/appointments", createRequest, JsonOptions);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);
        created.Should().NotBeNull();

        // Act
        var response = await _client.GetAsync($"/api/appointments/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var appointment = await response.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);
        appointment.Should().NotBeNull();
        appointment!.Id.Should().Be(created.Id);
        appointment.Title.Should().Be(createRequest.Title);
        appointment.Type.Should().Be(AppointmentType.OwnerMeeting);
    }

    [Fact]
    public async Task DeleteAppointment_ReturnsNoContent()
    {
        // Arrange: Create an appointment first
        var startTime = DateTime.UtcNow.AddHours(3);
        var endTime = startTime.AddHours(1);

        var createRequest = new
        {
            Title = "Appointment To Delete",
            Type = "PropertyViewing",
            StartTimeUtc = startTime,
            EndTimeUtc = endTime,
            PropertyId = Guid.NewGuid()
        };

        var createResponse = await _client.PostAsJsonAsync("/api/appointments", createRequest, JsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<AppointmentDto>(JsonOptions);
        created.Should().NotBeNull();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/appointments/{created!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetAppointmentById_ReturnsUnauthorized_WhenNoOrgClaim()
    {
        // Arrange: Send request with X-Test-Org-Id set to "none" to simulate missing org claim
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/appointments/{Guid.NewGuid()}");
        requestMessage.Headers.Add("X-Test-Org-Id", "none");

        // Act
        var response = await _client.SendAsync(requestMessage);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
