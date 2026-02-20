// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Propely.AiApi.Api.Dtos;
using Propely.AiApi.Application.Common.Models;
using Propely.AiApi.Application.WorkItems.Dtos;
using Propely.AiApi.Domain.WorkItems;
using Propely.AiApi.IntegrationTests.Fixtures;
using FluentAssertions;

namespace Propely.AiApi.IntegrationTests.TenantIsolation;

/// <summary>
/// Proves that tenant data isolation works correctly across all CRUD operations.
/// Each test creates data under one tenant and verifies another tenant cannot access it.
/// Uses X-Test-Org-Id header to impersonate different tenants via DevAuthenticationHandler.
/// </summary>
public sealed class WorkItemTenantIsolationTests : IClassFixture<ApiWebApplicationFactory>
{
    private static readonly Guid TenantA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid TenantB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client;

    public WorkItemTenantIsolationTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task List_TenantB_CannotSee_TenantA_Items()
    {
        // Arrange — create items as Tenant A
        var titlePrefix = $"IsoList_{Guid.NewGuid():N}";
        for (var i = 0; i < 3; i++)
        {
            await CreateWorkItemAs(TenantA, $"{titlePrefix}_{i}", "Desc");
        }

        // Wait for outbox projection to complete
        await WaitForProjectionAsync(TenantA, titlePrefix, expectedCount: 3);

        // Act — list as Tenant B
        var request = new HttpRequestMessage(HttpMethod.Get, "/v1/work-items?pageSize=50");
        request.Headers.Add("X-Test-Org-Id", TenantB.ToString());
        var response = await _client.SendAsync(request);

        // Assert — Tenant B sees none of Tenant A's items
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<WorkItemDto>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().NotContain(x => x.Title.StartsWith(titlePrefix));
    }

    [Fact]
    public async Task GetById_TenantB_Cannot_Access_TenantA_Item()
    {
        // Arrange — create item as Tenant A
        var itemId = await CreateWorkItemAs(TenantA, "IsoGet_A", "Desc");

        // Act — get by ID as Tenant B
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/work-items/{itemId}");
        request.Headers.Add("X-Test-Org-Id", TenantB.ToString());
        var response = await _client.SendAsync(request);

        // Assert — 404 (tenant filter hides the item)
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_TenantB_Cannot_Modify_TenantA_Item()
    {
        // Arrange — create item as Tenant A
        var itemId = await CreateWorkItemAs(TenantA, "IsoUpdate_A", "Original");

        // Act — update as Tenant B
        var request = new HttpRequestMessage(HttpMethod.Put, $"/v1/work-items/{itemId}");
        request.Headers.Add("X-Test-Org-Id", TenantB.ToString());
        request.Content = JsonContent.Create(new UpdateWorkItemRequest("Hacked", "Pwned", WorkItemStatus.Active));
        var response = await _client.SendAsync(request);

        // Assert — 404 (tenant filter hides the item)
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify — Tenant A's item is unchanged
        var verifyRequest = new HttpRequestMessage(HttpMethod.Get, $"/v1/work-items/{itemId}");
        verifyRequest.Headers.Add("X-Test-Org-Id", TenantA.ToString());
        var verifyResponse = await _client.SendAsync(verifyRequest);
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var item = await verifyResponse.Content.ReadFromJsonAsync<WorkItemResponse>();
        item!.Title.Should().Be("IsoUpdate_A");
        item.Description.Should().Be("Original");
    }

    [Fact]
    public async Task Delete_TenantB_Cannot_Delete_TenantA_Item()
    {
        // Arrange — create item as Tenant A
        var itemId = await CreateWorkItemAs(TenantA, "IsoDelete_A", "Desc");

        // Act — delete as Tenant B
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/v1/work-items/{itemId}");
        request.Headers.Add("X-Test-Org-Id", TenantB.ToString());
        var response = await _client.SendAsync(request);

        // Assert — 404 (tenant filter hides the item)
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify — Tenant A can still see the item
        var verifyRequest = new HttpRequestMessage(HttpMethod.Get, $"/v1/work-items/{itemId}");
        verifyRequest.Headers.Add("X-Test-Org-Id", TenantA.ToString());
        var verifyResponse = await _client.SendAsync(verifyRequest);
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_SetsCorrectOrgId_ForTenant()
    {
        // Act — create as Tenant A
        var itemId = await CreateWorkItemAs(TenantA, "IsoOrgId_A", "Desc");

        // Assert — the item's OrgId matches Tenant A
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/work-items/{itemId}");
        request.Headers.Add("X-Test-Org-Id", TenantA.ToString());
        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var item = await response.Content.ReadFromJsonAsync<WorkItemResponse>();
        item!.OrgId.Should().Be(TenantA);
    }

    [Fact]
    public async Task SoftDelete_PlusTenant_HidesFromBothTenants()
    {
        // Arrange — create item as Tenant A, then soft-delete it
        var itemId = await CreateWorkItemAs(TenantA, "IsoSoftDel_A", "Desc");

        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/v1/work-items/{itemId}");
        deleteRequest.Headers.Add("X-Test-Org-Id", TenantA.ToString());
        var deleteResponse = await _client.SendAsync(deleteRequest);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act + Assert — Tenant A cannot see soft-deleted item
        var getA = new HttpRequestMessage(HttpMethod.Get, $"/v1/work-items/{itemId}");
        getA.Headers.Add("X-Test-Org-Id", TenantA.ToString());
        var responseA = await _client.SendAsync(getA);
        responseA.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Act + Assert — Tenant B also cannot see it
        var getB = new HttpRequestMessage(HttpMethod.Get, $"/v1/work-items/{itemId}");
        getB.Headers.Add("X-Test-Org-Id", TenantB.ToString());
        var responseB = await _client.SendAsync(getB);
        responseB.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Pagination_RespectsTenantFilter()
    {
        // Arrange — create 5 items in Tenant A, 3 in Tenant B (with unique prefix)
        var prefix = $"IsoPaging_{Guid.NewGuid():N}";
        for (var i = 0; i < 5; i++)
        {
            await CreateWorkItemAs(TenantA, $"{prefix}_A{i}", "Desc");
        }
        for (var i = 0; i < 3; i++)
        {
            await CreateWorkItemAs(TenantB, $"{prefix}_B{i}", "Desc");
        }

        // Wait for outbox projection to complete for both tenants
        await WaitForProjectionAsync(TenantA, prefix, expectedCount: 5);
        await WaitForProjectionAsync(TenantB, prefix, expectedCount: 3);

        // Act — list as Tenant A
        var requestA = new HttpRequestMessage(HttpMethod.Get, "/v1/work-items?pageSize=50");
        requestA.Headers.Add("X-Test-Org-Id", TenantA.ToString());
        var responseA = await _client.SendAsync(requestA);
        responseA.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultA = await responseA.Content.ReadFromJsonAsync<PagedResult<WorkItemDto>>(JsonOptions);

        // Assert — Tenant A sees only its own items with this prefix
        var tenantAItems = resultA!.Items.Where(x => x.Title.StartsWith(prefix)).ToList();
        tenantAItems.Should().HaveCount(5);
        tenantAItems.Should().OnlyContain(x => x.Title.Contains("_A"));

        // Act — list as Tenant B
        var requestB = new HttpRequestMessage(HttpMethod.Get, "/v1/work-items?pageSize=50");
        requestB.Headers.Add("X-Test-Org-Id", TenantB.ToString());
        var responseB = await _client.SendAsync(requestB);
        responseB.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultB = await responseB.Content.ReadFromJsonAsync<PagedResult<WorkItemDto>>(JsonOptions);

        // Assert — Tenant B sees only its own items with this prefix
        var tenantBItems = resultB!.Items.Where(x => x.Title.StartsWith(prefix)).ToList();
        tenantBItems.Should().HaveCount(3);
        tenantBItems.Should().OnlyContain(x => x.Title.Contains("_B"));
    }

    [Fact]
    public async Task NoOrgIdClaim_ReturnsEmptyList_FailClosed()
    {
        // Arrange — create items as Tenant A
        var prefix = $"IsoNoClaim_{Guid.NewGuid():N}";
        await CreateWorkItemAs(TenantA, $"{prefix}_1", "Desc");

        // Wait for outbox projection to complete
        await WaitForProjectionAsync(TenantA, prefix, expectedCount: 1);

        // Act — list with no org_id claim (X-Test-Org-Id: none)
        var request = new HttpRequestMessage(HttpMethod.Get, "/v1/work-items?pageSize=50");
        request.Headers.Add("X-Test-Org-Id", "none");
        var response = await _client.SendAsync(request);

        // Assert — fail-closed: Guid.Empty matches no tenant, so empty list
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<WorkItemDto>>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetById_NoOrgIdClaim_Returns404_FailClosed()
    {
        // Arrange — create item as Tenant A
        var itemId = await CreateWorkItemAs(TenantA, "IsoNoClaimGet_A", "Desc");

        // Act — get by ID with no org_id claim
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/work-items/{itemId}");
        request.Headers.Add("X-Test-Org-Id", "none");
        var response = await _client.SendAsync(request);

        // Assert — fail-closed: no access
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task TenantA_CanStill_CRUD_OwnItems()
    {
        // This test ensures tenant filtering doesn't break normal operations
        // Create
        var itemId = await CreateWorkItemAs(TenantA, "IsoOwn_A", "Original");

        // Read
        var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/v1/work-items/{itemId}");
        getRequest.Headers.Add("X-Test-Org-Id", TenantA.ToString());
        var getResponse = await _client.SendAsync(getRequest);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var item = await getResponse.Content.ReadFromJsonAsync<WorkItemResponse>();
        item!.Title.Should().Be("IsoOwn_A");

        // Update
        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/v1/work-items/{itemId}");
        updateRequest.Headers.Add("X-Test-Org-Id", TenantA.ToString());
        updateRequest.Content = JsonContent.Create(new UpdateWorkItemRequest("IsoOwn_Updated", "Updated", WorkItemStatus.Active));
        var updateResponse = await _client.SendAsync(updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var verifyRequest = new HttpRequestMessage(HttpMethod.Get, $"/v1/work-items/{itemId}");
        verifyRequest.Headers.Add("X-Test-Org-Id", TenantA.ToString());
        var verifyResponse = await _client.SendAsync(verifyRequest);
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await verifyResponse.Content.ReadFromJsonAsync<WorkItemResponse>();
        updated!.Title.Should().Be("IsoOwn_Updated");
        updated.Status.Should().Be("Active");

        // Delete
        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/v1/work-items/{itemId}");
        deleteRequest.Headers.Add("X-Test-Org-Id", TenantA.ToString());
        var deleteResponse = await _client.SendAsync(deleteRequest);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deleted
        var deletedRequest = new HttpRequestMessage(HttpMethod.Get, $"/v1/work-items/{itemId}");
        deletedRequest.Headers.Add("X-Test-Org-Id", TenantA.ToString());
        var deletedResponse = await _client.SendAsync(deletedRequest);
        deletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Creates a work item as the specified tenant and returns the created item's ID.
    /// </summary>
    private async Task<Guid> CreateWorkItemAs(Guid tenantId, string title, string? description)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/v1/work-items");
        request.Headers.Add("X-Test-Org-Id", tenantId.ToString());
        request.Content = JsonContent.Create(new CreateWorkItemRequest(title, description));

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Created, $"Failed to create work item '{title}' for tenant {tenantId}");

        var item = await response.Content.ReadFromJsonAsync<WorkItemResponse>();
        return item!.Id;
    }

    /// <summary>
    /// Polls the list endpoint until the expected number of items with the given prefix appear,
    /// or a timeout is reached. This replaces fixed Task.Delay for outbox projection waits.
    /// </summary>
    private async Task WaitForProjectionAsync(Guid tenantId, string titlePrefix, int expectedCount)
    {
        var timeout = TimeSpan.FromSeconds(10);
        var interval = TimeSpan.FromMilliseconds(200);
        var start = DateTime.UtcNow;

        while (DateTime.UtcNow - start < timeout)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/v1/work-items?pageSize=50");
            request.Headers.Add("X-Test-Org-Id", tenantId.ToString());
            var response = await _client.SendAsync(request);
            var result = await response.Content.ReadFromJsonAsync<PagedResult<WorkItemDto>>(JsonOptions);

            var matchCount = result?.Items.Count(x => x.Title.StartsWith(titlePrefix)) ?? 0;
            if (matchCount >= expectedCount)
                return;

            await Task.Delay(interval);
        }
    }
}
