// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Interfaces;
using Propely.OrgsApi.Application.Users.Interfaces;
using Propely.OrgsApi.Domain.Common.Exceptions;
using MediatR;

namespace Propely.OrgsApi.Application.Users.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UpdateProfileResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProfileCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateProfileResult> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"User {request.UserId} not found.");

        user.UpdateProfile(request.Name);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateProfileResult(user.Id, user.Email, user.Name);
    }
}
