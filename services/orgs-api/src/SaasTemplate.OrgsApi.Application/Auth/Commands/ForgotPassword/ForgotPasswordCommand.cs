// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace SaasTemplate.OrgsApi.Application.Auth.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(
    string Email,
    string FrontendBaseUrl,
    string Locale = "en") : IRequest;
