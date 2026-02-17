// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using FluentValidation;

namespace SaasTemplate.OrgsApi.Application.FeatureFlags.Commands.ToggleFeatureFlag;

public sealed class ToggleFeatureFlagCommandValidator : AbstractValidator<ToggleFeatureFlagCommand>
{
    public ToggleFeatureFlagCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Feature flag name is required.")
            .MaximumLength(200).WithMessage("Feature flag name must not exceed 200 characters.");
    }
}
