// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Domain.Common;

namespace SaasTemplate.OrgsApi.Domain.Notifications;

public sealed class Notification : Entity
{
    public const int TitleMaxLength = 200;
    public const int BodyMaxLength = 1000;
    public const int MetadataMaxLength = 4000;

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public bool IsRead { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public string? Metadata { get; private set; }

    private Notification() { }

    public static Notification Create(
        Guid userId,
        NotificationType type,
        string title,
        string body,
        string? metadata = null)
    {
        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title.Trim(),
            Body = body.Trim(),
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow,
            Metadata = metadata
        };
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
