// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.PropertiesApi.Application.Properties.Dtos;

namespace Propely.PropertiesApi.Application.Properties.Queries.GetPropertyById;

public sealed record GetPropertyByIdQuery(Guid PropertyId, Guid TenantId) : IRequest<PropertyDto?>;
