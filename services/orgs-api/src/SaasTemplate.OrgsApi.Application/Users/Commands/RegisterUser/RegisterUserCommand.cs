// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace SaasTemplate.OrgsApi.Application.Users.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string? Name,
    string FrontendBaseUrl,
    string Locale) : IRequest<RegisterUserResult>;

public sealed record RegisterUserResult(Guid UserId, string Token, string RefreshToken);
