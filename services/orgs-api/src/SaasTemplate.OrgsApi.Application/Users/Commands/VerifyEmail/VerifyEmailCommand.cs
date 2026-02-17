// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace SaasTemplate.OrgsApi.Application.Users.Commands.VerifyEmail;

public sealed record VerifyEmailCommand(string Token) : IRequest<VerifyEmailResult>;

public sealed record VerifyEmailResult(bool EmailVerified);
