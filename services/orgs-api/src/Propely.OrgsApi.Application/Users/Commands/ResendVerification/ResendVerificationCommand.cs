// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.OrgsApi.Application.Users.Commands.ResendVerification;

public sealed record ResendVerificationCommand(
    Guid UserId,
    string FrontendBaseUrl,
    string Locale) : IRequest<ResendVerificationResult>;

public sealed record ResendVerificationResult(bool Sent, bool AlreadyVerified);
