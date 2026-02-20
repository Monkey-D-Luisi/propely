// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Common;

namespace Propely.OrgsApi.Domain.Users;

public sealed class UserExternalLogin : Entity
{
    public const int ProviderMaxLength = 50;
    public const int ExternalIdMaxLength = 256;

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Provider { get; private set; } = null!;
    public string ExternalId { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }

    private UserExternalLogin()
    {
    }

    public static UserExternalLogin Create(Guid userId, string provider, string externalId)
    {
        return new UserExternalLogin
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = NormalizeProvider(provider),
            ExternalId = NormalizeExternalId(externalId),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private static string NormalizeProvider(string provider) => provider.Trim().ToLowerInvariant();

    private static string NormalizeExternalId(string externalId) => externalId.Trim();
}
