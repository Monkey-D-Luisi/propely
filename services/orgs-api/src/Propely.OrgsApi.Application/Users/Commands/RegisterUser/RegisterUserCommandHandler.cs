// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Email;
using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Users;
using Propely.OrgsApi.Application.Users.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Users;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Propely.OrgsApi.Application.Users.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException("EMAIL_TAKEN");
        }

        var hash = _passwordHasher.HashPassword(request.Password);
        var user = User.Create(request.Email, hash, request.Name);

        await _userRepository.AddAsync(user, cancellationToken);

        var (refreshTokenEntity, plainRefreshToken) = RefreshToken.Create(user.Id, RefreshTokenLifetime);
        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var verificationToken = _jwtTokenService.GenerateEmailVerificationToken(user.Id, user.Email);
        var verificationUrl = EmailVerificationLinkBuilder.Build(
            request.FrontendBaseUrl,
            request.Locale,
            verificationToken);

        var normalizedBaseUrl = request.FrontendBaseUrl.Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(normalizedBaseUrl))
        {
            _logger.LogWarning(
                "FrontendBaseUrl not configured, falling back to localhost for user {UserId}",
                user.Id);
            normalizedBaseUrl = "http://localhost:3000";
        }

        var normalizedLocale = InvitationEmailLocalization.NormalizeLocale(request.Locale);
        var loginUrl = $"{normalizedBaseUrl}/{normalizedLocale}/login";

        try
        {
            await _emailService.SendEmailVerificationEmailAsync(
                user.Email,
                verificationUrl,
                cancellationToken,
                request.Locale);

            await _emailService.SendWelcomeEmailAsync(
                user.Email,
                user.Name ?? user.Email,
                loginUrl,
                cancellationToken,
                request.Locale);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send registration emails for user {UserId}", user.Id);
        }

        var token = _jwtTokenService.GenerateToken(user, Array.Empty<Guid>());

        return new RegisterUserResult(user.Id, token, plainRefreshToken);
    }
}
