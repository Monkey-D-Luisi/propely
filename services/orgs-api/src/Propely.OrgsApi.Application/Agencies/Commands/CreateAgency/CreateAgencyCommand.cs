// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.OrgsApi.Application.Agencies.Commands.CreateAgency;

public sealed record CreateAgencyCommand(string Name, string Slug, Guid CreatedByUserId) : IRequest<CreateAgencyResult>;

public sealed record CreateAgencyResult(Guid AgencyId, string Name, string Slug);
