// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.OrgsApi.Application.Organizations.Commands.CreateOrganization;

public sealed record CreateOrganizationCommand(string Name, Guid OwnerUserId) : IRequest<CreateOrganizationResult>;

public sealed record CreateOrganizationResult(Guid OrgId, string Name);
