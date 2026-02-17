// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace SaasTemplate.OrgsApi.Application.Users.Commands.OAuthLogin;

public sealed record OAuthLoginCommand(
    string Email,
    string? Name,
    string Provider,
    string ExternalId) : IRequest<OAuthLoginResult>;

public sealed record OAuthLoginResult(Guid UserId, string Token, string RefreshToken);
