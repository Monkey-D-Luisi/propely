// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.AiApi.Application.WorkItems.Commands;

public record DeleteWorkItemCommand(Guid Id, Guid UserId) : IRequest;
