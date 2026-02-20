// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Users;
using Propely.OrgsApi.Application.Users.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using MediatR;

namespace Propely.OrgsApi.Application.Users.Commands.ResendVerification;

public sealed class ResendVerificationCommandHandler : IRequestHandler<ResendVerificationCommand, ResendVerificationResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;

    public ResendVerificationCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
    }

    public async Task<ResendVerificationResult> Handle(
        ResendVerificationCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("USER_NOT_FOUND");

        if (user.EmailVerified)
        {
            return new ResendVerificationResult(Sent: false, AlreadyVerified: true);
        }

        var token = _jwtTokenService.GenerateEmailVerificationToken(user.Id, user.Email);
        var verificationUrl = EmailVerificationLinkBuilder.Build(request.FrontendBaseUrl, request.Locale, token);

        await _emailService.SendEmailVerificationEmailAsync(
            user.Email,
            verificationUrl,
            cancellationToken,
            request.Locale);

        return new ResendVerificationResult(Sent: true, AlreadyVerified: false);
    }
}
