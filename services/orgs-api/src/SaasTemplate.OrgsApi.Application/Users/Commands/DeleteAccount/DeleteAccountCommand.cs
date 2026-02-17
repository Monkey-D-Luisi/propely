// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace SaasTemplate.OrgsApi.Application.Users.Commands.DeleteAccount;

public sealed record DeleteAccountCommand(
    Guid UserId,
    string Password) : IRequest;
