// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.OrgsApi.Application.Users.Commands.RefreshAccessToken;

public sealed record RefreshAccessTokenCommand(string RefreshToken) : IRequest<RefreshAccessTokenResult>;

public sealed record RefreshAccessTokenResult(string AccessToken, string RefreshToken);
