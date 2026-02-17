// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Auth.Security;
using SaasTemplate.OrgsApi.Application.Common.Email;
using SaasTemplate.OrgsApi.Application.Common.Interfaces;
using SaasTemplate.OrgsApi.Application.Users.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace SaasTemplate.OrgsApi.Application.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            _logger.LogInformation("Security: Password reset requested for non-existent email");
            return;
        }

        var fingerprint = PasswordResetTokenFingerprint.FromPasswordHash(user.PasswordHash);
        var token = _jwtTokenService.GeneratePasswordResetToken(user.Id, user.Email, fingerprint);
        var resetUrl = BuildResetUrl(token, request.FrontendBaseUrl, request.Locale);
        var locale = InvitationEmailLocalization.NormalizeLocale(request.Locale);

        await _emailService.SendPasswordResetEmailAsync(
            user.Email,
            resetUrl,
            cancellationToken,
            locale);

        _logger.LogInformation("Security: Password reset token generated for user {UserId}", user.Id);
    }

    private static string BuildResetUrl(string token, string frontendBaseUrl, string locale)
    {
        var normalizedBaseUrl = string.IsNullOrWhiteSpace(frontendBaseUrl)
            ? "http://localhost:3000"
            : frontendBaseUrl.TrimEnd('/');
        var normalizedLocale = InvitationEmailLocalization.NormalizeLocale(locale);
        var encodedToken = Uri.EscapeDataString(token);

        return $"{normalizedBaseUrl}/{normalizedLocale}/reset-password?token={encodedToken}";
    }
}
