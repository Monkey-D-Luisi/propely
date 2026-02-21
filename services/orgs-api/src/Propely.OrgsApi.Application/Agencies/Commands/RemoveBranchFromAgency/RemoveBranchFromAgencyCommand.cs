// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.OrgsApi.Application.Agencies.Commands.RemoveBranchFromAgency;

public sealed record RemoveBranchFromAgencyCommand(Guid AgencyId, Guid OrganizationId, Guid RequestingUserId) : IRequest;
