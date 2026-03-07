// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Organizations.Interfaces;
using Propely.OrgsApi.Application.Users.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using Propely.OrgsApi.Domain.Users;
using MediatR;

namespace Propely.OrgsApi.Application.Users.Commands.OAuthLogin;

public sealed class OAuthLoginCommandHandler : IRequestHandler<OAuthLoginCommand, OAuthLoginResult>
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    private readonly IUserRepository _userRepository;
    private readonly IUserExternalLoginRepository _userExternalLoginRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OAuthLoginCommandHandler(
        IUserRepository userRepository,
        IUserExternalLoginRepository userExternalLoginRepository,
        IMembershipRepository membershipRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _userExternalLoginRepository = userExternalLoginRepository;
        _membershipRepository = membershipRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OAuthLoginResult> Handle(OAuthLoginCommand request, CancellationToken cancellationToken)
    {
        var linkedLogin = await _userExternalLoginRepository.GetByProviderAndExternalIdAsync(
            request.Provider,
            request.ExternalId,
            cancellationToken);

        if (linkedLogin is not null)
        {
            var linkedUser = await _userRepository.GetByIdAsync(linkedLogin.UserId, cancellationToken)
                ?? throw new InvalidOperationException("Linked user not found.");

            var linkedMemberships = await _membershipRepository.GetByUserIdAsync(linkedUser.Id, cancellationToken);
            var linkedOrgIds = linkedMemberships.Select(m => m.OrganizationId).ToList();
            var linkedRoles = linkedMemberships.Select(m => m.Role.ToString().ToLowerInvariant()).ToList();
            var linkedToken = _jwtTokenService.GenerateToken(linkedUser, linkedOrgIds, linkedRoles);

            var (linkedRefreshEntity, linkedPlainRefresh) = RefreshToken.Create(linkedUser.Id, RefreshTokenLifetime);
            await _refreshTokenRepository.AddAsync(linkedRefreshEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new OAuthLoginResult(linkedUser.Id, linkedToken, linkedPlainRefresh);
        }

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
        {
            var randomPassword = Guid.NewGuid().ToString("N");
            var generatedHash = _passwordHasher.HashPassword(randomPassword);
            user = User.Create(request.Email, generatedHash, request.Name, emailVerified: true);

            await _userRepository.AddAsync(user, cancellationToken);
        }
        else if (!user.EmailVerified)
        {
            user.MarkEmailAsVerified();
        }

        var providerLinkForUser = await _userExternalLoginRepository.GetByUserIdAndProviderAsync(
            user.Id,
            request.Provider,
            cancellationToken);

        if (providerLinkForUser is not null &&
            !string.Equals(providerLinkForUser.ExternalId, request.ExternalId, StringComparison.Ordinal))
        {
            throw new ConflictException("PROVIDER_ALREADY_LINKED");
        }

        if (providerLinkForUser is null)
        {
            var link = UserExternalLogin.Create(user.Id, request.Provider, request.ExternalId);
            await _userExternalLoginRepository.AddAsync(link, cancellationToken);
        }

        var (refreshTokenEntity, plainRefreshToken) = RefreshToken.Create(user.Id, RefreshTokenLifetime);
        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var memberships = await _membershipRepository.GetByUserIdAsync(user.Id, cancellationToken);
        var orgIds = memberships.Select(m => m.OrganizationId).ToList();
        var roles = memberships.Select(m => m.Role.ToString().ToLowerInvariant()).ToList();
        var token = _jwtTokenService.GenerateToken(user, orgIds, roles);
        return new OAuthLoginResult(user.Id, token, plainRefreshToken);
    }
}
