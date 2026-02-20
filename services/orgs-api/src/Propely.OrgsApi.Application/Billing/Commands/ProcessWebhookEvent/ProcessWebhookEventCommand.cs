// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.OrgsApi.Application.Billing.Commands.ProcessWebhookEvent;

public sealed record ProcessWebhookEventCommand(
    string EventId,
    string EventType,
    WebhookEventData EventData) : IRequest;
