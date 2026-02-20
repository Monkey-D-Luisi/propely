// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.OrgsApi.Application.Organizations.Commands.DeleteOrganization;

public sealed record DeleteOrganizationCommand(Guid OrgId, Guid UserId) : IRequest;
