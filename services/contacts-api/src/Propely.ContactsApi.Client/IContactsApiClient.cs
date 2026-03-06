// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Client.Models;
using Refit;

namespace Propely.ContactsApi.Client;

/// <summary>
/// Refit-based typed HTTP client for the Propely Contacts API.
/// Provides access to contact data for cross-service communication.
/// </summary>
public interface IContactsApiClient
{
    /// <summary>
    /// Lists contacts with optional filtering, sorting, and pagination.
    /// </summary>
    /// <param name="search">Full-text search across name and email fields.</param>
    /// <param name="role">Filter by contact role.</param>
    /// <param name="sortBy">Sort field name.</param>
    /// <param name="sortDesc">Sort in descending order.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A paged list of contacts.</returns>
    [Get("/api/contacts")]
    Task<ContactListResponse> GetContactsAsync(
        [Query] string? search = null,
        [Query] string? role = null,
        [Query] string? sortBy = null,
        [Query] bool sortDesc = false,
        [Query] int page = 1,
        [Query] int pageSize = 20,
        CancellationToken ct = default);

    /// <summary>
    /// Gets a contact by its unique identifier.
    /// </summary>
    /// <param name="id">The contact ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The contact details.</returns>
    [Get("/api/contacts/{id}")]
    Task<ContactResponse> GetContactByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Creates a new contact.
    /// </summary>
    /// <param name="request">The contact creation request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created contact.</returns>
    [Post("/api/contacts")]
    Task<ContactResponse> CreateContactAsync([Body] CreateContactClientRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing contact.
    /// </summary>
    /// <param name="id">The contact ID.</param>
    /// <param name="request">The contact update request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated contact.</returns>
    [Put("/api/contacts/{id}")]
    Task<ContactResponse> UpdateContactAsync(Guid id, [Body] UpdateContactClientRequest request, CancellationToken ct = default);

    /// <summary>
    /// Deletes a contact.
    /// </summary>
    /// <param name="id">The contact ID.</param>
    /// <param name="ct">Cancellation token.</param>
    [Delete("/api/contacts/{id}")]
    Task DeleteContactAsync(Guid id, CancellationToken ct = default);
}
