// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Client.Models;
using Refit;

namespace Propely.AppointmentsApi.Client;

/// <summary>
/// Refit-based typed HTTP client for the Propely Appointments API.
/// Provides access to appointments data for cross-service communication.
/// </summary>
public interface IAppointmentsApiClient
{
    /// <summary>
    /// Lists appointments with optional filtering, sorting, and pagination.
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="search">Full-text search across title and description fields.</param>
    /// <param name="status">Filter by appointment status (e.g., Scheduled, Confirmed, Completed, Cancelled, NoShow).</param>
    /// <param name="type">Filter by appointment type (e.g., PropertyViewing, OwnerMeeting, Generic).</param>
    /// <param name="agentId">Filter by agent ID.</param>
    /// <param name="propertyId">Filter by property ID.</param>
    /// <param name="contactId">Filter by contact ID.</param>
    /// <param name="fromUtc">Filter appointments starting from this UTC date/time.</param>
    /// <param name="toUtc">Filter appointments up to this UTC date/time.</param>
    /// <param name="sortBy">Sort field name.</param>
    /// <param name="sortDescending">Sort in descending order.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A paged list of appointments.</returns>
    [Get("/api/appointments")]
    Task<AppointmentListResponse> GetAppointmentsAsync(
        [Query] int page = 1,
        [Query] int pageSize = 20,
        [Query] string? search = null,
        [Query] string? status = null,
        [Query] string? type = null,
        [Query] Guid? agentId = null,
        [Query] Guid? propertyId = null,
        [Query] Guid? contactId = null,
        [Query] DateTime? fromUtc = null,
        [Query] DateTime? toUtc = null,
        [Query] string? sortBy = null,
        [Query] bool sortDescending = false,
        CancellationToken ct = default);

    /// <summary>
    /// Gets an appointment by its unique identifier.
    /// </summary>
    /// <param name="id">The appointment ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The appointment details.</returns>
    [Get("/api/appointments/{id}")]
    Task<AppointmentResponse> GetAppointmentByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets the count of appointments grouped by status.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A dictionary of status names to their counts.</returns>
    [Get("/api/appointments/count-by-status")]
    Task<Dictionary<string, int>> CountByStatusAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the count of upcoming appointments within the specified number of days.
    /// </summary>
    /// <param name="days">Number of days to look ahead (default 7).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An object containing the count.</returns>
    [Get("/api/appointments/count-upcoming")]
    Task<CountUpcomingResponse> CountUpcomingAsync([Query] int days = 7, CancellationToken ct = default);

    /// <summary>
    /// Creates a new appointment.
    /// </summary>
    /// <param name="request">The appointment creation request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created appointment.</returns>
    [Post("/api/appointments")]
    Task<AppointmentResponse> CreateAppointmentAsync([Body] CreateAppointmentClientRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing appointment.
    /// </summary>
    /// <param name="id">The appointment ID.</param>
    /// <param name="request">The appointment update request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated appointment.</returns>
    [Put("/api/appointments/{id}")]
    Task<AppointmentResponse> UpdateAppointmentAsync(Guid id, [Body] UpdateAppointmentClientRequest request, CancellationToken ct = default);

    /// <summary>
    /// Deletes an appointment.
    /// </summary>
    /// <param name="id">The appointment ID.</param>
    /// <param name="ct">Cancellation token.</param>
    [Delete("/api/appointments/{id}")]
    Task DeleteAppointmentAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Confirms a scheduled appointment.
    /// </summary>
    /// <param name="id">The appointment ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The confirmed appointment.</returns>
    [Put("/api/appointments/{id}/confirm")]
    Task<AppointmentResponse> ConfirmAppointmentAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Completes an appointment with optional notes.
    /// </summary>
    /// <param name="id">The appointment ID.</param>
    /// <param name="request">The completion request with optional notes.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The completed appointment.</returns>
    [Put("/api/appointments/{id}/complete")]
    Task<AppointmentResponse> CompleteAppointmentAsync(Guid id, [Body] CompleteAppointmentClientRequest request, CancellationToken ct = default);

    /// <summary>
    /// Cancels an appointment with a reason.
    /// </summary>
    /// <param name="id">The appointment ID.</param>
    /// <param name="request">The cancellation request with reason.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The cancelled appointment.</returns>
    [Put("/api/appointments/{id}/cancel")]
    Task<AppointmentResponse> CancelAppointmentAsync(Guid id, [Body] CancelAppointmentClientRequest request, CancellationToken ct = default);

    /// <summary>
    /// Marks an appointment as no-show.
    /// </summary>
    /// <param name="id">The appointment ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated appointment.</returns>
    [Put("/api/appointments/{id}/no-show")]
    Task<AppointmentResponse> MarkNoShowAsync(Guid id, CancellationToken ct = default);
}
