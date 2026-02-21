// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.OrgsApi.Application.Agencies.Commands.AddBranchToAgency;

public sealed record AddBranchToAgencyCommand(Guid AgencyId, Guid OrganizationId, Guid RequestingUserId) : IRequest;
