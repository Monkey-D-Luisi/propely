// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Application.Users.Interfaces;
using Propely.OrgsApi.Domain.Users;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Propely.OrgsApi.Application.Users.Commands.RefreshAccessToken;

public sealed class RefreshAccessTokenCommandHandler
    : IRequestHandler<RefreshAccessTokenCommand, RefreshAccessTokenResult>
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RefreshAccessTokenCommandHandler> _logger;

    public RefreshAccessTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IMembershipRepository membershipRepository,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        ILogger<RefreshAccessTokenCommandHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _membershipRepository = membershipRepository;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<RefreshAccessTokenResult> Handle(
        RefreshAccessTokenCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash = RefreshToken.HashToken(request.RefreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (existingToken is null || !existingToken.IsActive)
        {
            _logger.LogWarning("Security: Refresh token validation failed (token not found or inactive)");
            throw new UnauthorizedAccessException("INVALID_REFRESH_TOKEN");
        }

        var user = await _userRepository.GetByIdAsync(existingToken.UserId, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Security: Refresh token references non-existent user {UserId}", existingToken.UserId);
            throw new UnauthorizedAccessException("INVALID_REFRESH_TOKEN");
        }

        // Rotate: revoke old token and create new one
        var (newRefreshToken, newPlainToken) = RefreshToken.Create(user.Id, RefreshTokenLifetime);
        existingToken.Revoke(replacedByTokenId: newRefreshToken.Id);

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var memberships = await _membershipRepository.GetByUserIdAsync(user.Id, cancellationToken);
        var orgIds = memberships.Select(m => m.OrganizationId).ToList();
        var accessToken = _jwtTokenService.GenerateToken(user, orgIds);

        _logger.LogInformation("Security: Refresh token rotated for user {UserId}", user.Id);
        return new RefreshAccessTokenResult(accessToken, newPlainToken);
    }
}
