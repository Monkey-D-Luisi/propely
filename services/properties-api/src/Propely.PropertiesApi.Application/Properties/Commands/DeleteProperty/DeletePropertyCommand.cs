// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.PropertiesApi.Application.Properties.Commands.DeleteProperty;

public sealed record DeletePropertyCommand(Guid PropertyId, Guid TenantId) : IRequest;
