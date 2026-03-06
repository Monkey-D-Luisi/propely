// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Domain.Common;
using Propely.AppointmentsApi.Domain.Common.Exceptions;

namespace Propely.AppointmentsApi.Domain.Appointments;

public sealed class CalendarConnection : Entity, ISoftDeletable
{
    public const int ExternalCalendarIdMaxLength = 500;
    public const int LastSyncErrorMaxLength = 2000;
    public const int EncryptedTokenMaxLength = 4000;

    public Guid Id { get; private set; }
    public Guid AgentId { get; private set; }
    public Guid TenantId { get; private set; }
    public CalendarProvider Provider { get; private set; }
    public string EncryptedAccessToken { get; private set; } = null!;
    public string EncryptedRefreshToken { get; private set; } = null!;
    public DateTime TokenExpiresAtUtc { get; private set; }
    public string ExternalCalendarId { get; private set; } = null!;
    public DateTime? LastSyncedUtc { get; private set; }
    public string? LastSyncError { get; private set; }
    public CalendarSyncState SyncState { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    private CalendarConnection() { }

    public static CalendarConnection Create(
        Guid agentId,
        Guid tenantId,
        CalendarProvider provider,
        string encryptedAccessToken,
        string encryptedRefreshToken,
        DateTime tokenExpiresAtUtc,
        string externalCalendarId)
    {
        if (agentId == Guid.Empty)
            throw new DomainException("Agent ID is required.");

        if (tenantId == Guid.Empty)
            throw new DomainException("Tenant ID is required.");

        if (string.IsNullOrWhiteSpace(encryptedAccessToken))
            throw new DomainException("Encrypted access token is required.");

        if (string.IsNullOrWhiteSpace(encryptedRefreshToken))
            throw new DomainException("Encrypted refresh token is required.");

        if (string.IsNullOrWhiteSpace(externalCalendarId))
            throw new DomainException("External calendar ID is required.");

        var now = DateTime.UtcNow;

        return new CalendarConnection
        {
            Id = Guid.NewGuid(),
            AgentId = agentId,
            TenantId = tenantId,
            Provider = provider,
            EncryptedAccessToken = encryptedAccessToken,
            EncryptedRefreshToken = encryptedRefreshToken,
            TokenExpiresAtUtc = tokenExpiresAtUtc,
            ExternalCalendarId = externalCalendarId,
            SyncState = CalendarSyncState.Active,
            CreatedAtUtc = now
        };
    }

    public void UpdateTokens(string encryptedAccessToken, string encryptedRefreshToken, DateTime tokenExpiresAtUtc)
    {
        if (string.IsNullOrWhiteSpace(encryptedAccessToken))
            throw new DomainException("Encrypted access token is required.");

        if (string.IsNullOrWhiteSpace(encryptedRefreshToken))
            throw new DomainException("Encrypted refresh token is required.");

        EncryptedAccessToken = encryptedAccessToken;
        EncryptedRefreshToken = encryptedRefreshToken;
        TokenExpiresAtUtc = tokenExpiresAtUtc;
        UpdatedAtUtc = DateTime.UtcNow;

        if (SyncState == CalendarSyncState.Error)
        {
            SyncState = CalendarSyncState.Active;
            LastSyncError = null;
        }
    }

    public void MarkSynced()
    {
        var now = DateTime.UtcNow;
        LastSyncedUtc = now;
        LastSyncError = null;
        SyncState = CalendarSyncState.Active;
        UpdatedAtUtc = now;
    }

    public void MarkError(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
            throw new DomainException("Error message is required.");

        var truncated = error.Length > LastSyncErrorMaxLength
            ? error[..LastSyncErrorMaxLength]
            : error;

        LastSyncError = truncated;
        SyncState = CalendarSyncState.Error;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Disable()
    {
        SyncState = CalendarSyncState.Disabled;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        var now = DateTime.UtcNow;
        IsDeleted = true;
        DeletedAtUtc = now;
        UpdatedAtUtc = now;
        SyncState = CalendarSyncState.Disabled;
    }
}
