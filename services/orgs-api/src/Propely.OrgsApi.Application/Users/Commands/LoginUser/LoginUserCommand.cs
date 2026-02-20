// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.OrgsApi.Application.Users.Commands.LoginUser;

public sealed record LoginUserCommand(string Email, string Password) : IRequest<LoginUserResult>;

public sealed record LoginUserResult(Guid UserId, string Token, string RefreshToken);
