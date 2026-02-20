// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Domain.Users;

namespace Propely.OrgsApi.Application.Users.Interfaces;

public sealed record EmailVerificationTokenPayload(Guid UserId, string Email);

public sealed record PasswordResetTokenPayload(
    Guid UserId,
    string Email,
    string PasswordHashFingerprint);

public interface IJwtTokenService
{
    string GenerateToken(User user, IReadOnlyList<Guid> orgIds);

    string GenerateEmailVerificationToken(Guid userId, string email);

    EmailVerificationTokenPayload? ValidateEmailVerificationToken(string token);

    string GeneratePasswordResetToken(Guid userId, string email, string passwordHashFingerprint);

    PasswordResetTokenPayload? ValidatePasswordResetToken(string token);
}
