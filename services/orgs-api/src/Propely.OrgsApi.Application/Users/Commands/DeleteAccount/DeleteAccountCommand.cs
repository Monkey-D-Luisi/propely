// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;

namespace Propely.OrgsApi.Application.Users.Commands.DeleteAccount;

public sealed record DeleteAccountCommand(
    Guid UserId,
    string Password) : IRequest;
