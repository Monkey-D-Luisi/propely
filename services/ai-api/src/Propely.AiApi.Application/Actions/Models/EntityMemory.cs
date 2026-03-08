// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Application.Actions.Models;

/// <summary>
/// Tracks the most recently referenced entity IDs in a conversation session.
/// Used by the classifier to resolve references like "it", "that property", "the lead".
/// </summary>
public sealed record EntityMemory
{
    public Guid? LastPropertyId { get; init; }
    public Guid? LastContactId { get; init; }
    public Guid? LastLeadId { get; init; }
    public Guid? LastAppointmentId { get; init; }

    public static EntityMemory Empty => new();
}
