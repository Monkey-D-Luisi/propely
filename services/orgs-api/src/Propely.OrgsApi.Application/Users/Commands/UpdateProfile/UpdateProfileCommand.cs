// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.OrgsApi.Application.Users.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(Guid UserId, string? Name) : IRequest<UpdateProfileResult>;

public sealed record UpdateProfileResult(Guid Id, string Email, string? Name);
