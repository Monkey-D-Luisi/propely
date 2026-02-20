// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Users.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using MediatR;

namespace Propely.OrgsApi.Application.Users.Commands.VerifyEmail;

public sealed class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, VerifyEmailResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyEmailCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<VerifyEmailResult> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var payload = _jwtTokenService.ValidateEmailVerificationToken(request.Token);
        if (payload is null)
        {
            throw new DomainException("INVALID_OR_EXPIRED_VERIFICATION_TOKEN");
        }

        var user = await _userRepository.GetByIdAsync(payload.UserId, cancellationToken)
            ?? throw new NotFoundException("USER_NOT_FOUND");

        if (!string.Equals(user.Email, payload.Email, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("INVALID_OR_EXPIRED_VERIFICATION_TOKEN");
        }

        user.MarkEmailAsVerified();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new VerifyEmailResult(user.EmailVerified);
    }
}
