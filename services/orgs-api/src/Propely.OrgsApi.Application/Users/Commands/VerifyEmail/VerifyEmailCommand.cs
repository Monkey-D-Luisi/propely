// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.OrgsApi.Application.Users.Commands.VerifyEmail;

public sealed record VerifyEmailCommand(string Token) : IRequest<VerifyEmailResult>;

public sealed record VerifyEmailResult(bool EmailVerified);
