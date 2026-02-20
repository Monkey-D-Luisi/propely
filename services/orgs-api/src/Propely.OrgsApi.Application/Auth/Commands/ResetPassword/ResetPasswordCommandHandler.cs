// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Auth.Security;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Users.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Propely.OrgsApi.Application.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var tokenPayload = _jwtTokenService.ValidatePasswordResetToken(request.Token);
        if (tokenPayload is null)
        {
            _logger.LogWarning("Security: Invalid or expired password reset token submitted");
            throw new DomainException("INVALID_OR_EXPIRED_RESET_TOKEN");
        }

        var user = await _userRepository.GetByIdAsync(tokenPayload.UserId, cancellationToken);
        if (user is null || !string.Equals(user.Email, tokenPayload.Email, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Security: Password reset token for non-existent or mismatched user {UserId}", tokenPayload.UserId);
            throw new DomainException("INVALID_OR_EXPIRED_RESET_TOKEN");
        }

        var currentFingerprint = PasswordResetTokenFingerprint.FromPasswordHash(user.PasswordHash);
        if (!string.Equals(currentFingerprint, tokenPayload.PasswordHashFingerprint, StringComparison.Ordinal))
        {
            _logger.LogWarning("Security: Attempted reuse of password reset token for user {UserId}", user.Id);
            throw new DomainException("RESET_TOKEN_ALREADY_USED");
        }

        var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.ChangePassword(newPasswordHash);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Security: Password reset completed for user {UserId}", user.Id);
    }
}
