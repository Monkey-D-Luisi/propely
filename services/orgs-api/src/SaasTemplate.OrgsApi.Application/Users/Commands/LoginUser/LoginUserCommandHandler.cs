// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Application.Common.Interfaces;
using SaasTemplate.OrgsApi.Application.Organizations.Interfaces;
using SaasTemplate.OrgsApi.Application.Users.Interfaces;
using SaasTemplate.OrgsApi.Domain.Users;
using MediatR;
using Microsoft.Extensions.Logging;

namespace SaasTemplate.OrgsApi.Application.Users.Commands.LoginUser;

public sealed class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginUserResult>
{
    // Pre-computed bcrypt hash used to consume constant time when user is not found,
    // preventing timing-based email enumeration.
    private const string TimingSafetyHash = "$2a$11$3rDVCgfFeDtTjkD22YyaAeScMV0wyu0nFxo5oRLnCkbTp1YFaqcy6";

    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    private readonly IUserRepository _userRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LoginUserCommandHandler> _logger;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IMembershipRepository membershipRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        ILogger<LoginUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _membershipRepository = membershipRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<LoginUserResult> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        // Always run bcrypt verification to prevent timing-based email enumeration.
        // When user is null, verify against a dummy hash so the response time is consistent.
        var hashToVerify = user?.PasswordHash ?? TimingSafetyHash;
        var isValid = _passwordHasher.VerifyPassword(request.Password, hashToVerify) && user is not null;

        if (!isValid)
        {
            _logger.LogWarning("Security: Failed login attempt for email {Email}", MaskEmail(request.Email));
            throw new UnauthorizedAccessException("INVALID_CREDENTIALS");
        }

        _logger.LogInformation("Security: Successful login for user {UserId}", user!.Id);

        var memberships = await _membershipRepository.GetByUserIdAsync(user!.Id, cancellationToken);
        var orgIds = memberships.Select(m => m.OrganizationId).ToList();
        var token = _jwtTokenService.GenerateToken(user!, orgIds);

        var (refreshTokenEntity, plainRefreshToken) = RefreshToken.Create(user!.Id, RefreshTokenLifetime);
        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginUserResult(user!.Id, token, plainRefreshToken);
    }

    private static string MaskEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        if (atIndex <= 1)
            return "***@***";

        return string.Concat(email.AsSpan(0, 1), "***", email.AsSpan(atIndex));
    }
}
