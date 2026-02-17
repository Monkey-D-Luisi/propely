// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using SaasTemplate.OrgsApi.Api.Configuration;
using SaasTemplate.OrgsApi.Application.FeatureFlags.Commands.ToggleFeatureFlag;
using SaasTemplate.OrgsApi.Application.FeatureFlags.Queries.CheckFeatureFlag;
using SaasTemplate.OrgsApi.Application.FeatureFlags.Queries.GetFeatureFlags;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaasTemplate.OrgsApi.Api.Controllers;

[ApiController]
[Route("feature-flags")]
[Authorize]
public sealed class FeatureFlagsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FeatureFlagsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFeatureFlagsQuery(), cancellationToken);

        return Ok(new
        {
            flags = result.Flags.Select(f => new
            {
                f.Name,
                f.IsEnabled,
                f.Description,
                f.Source
            })
        });
    }

    [HttpGet("{name}")]
    public async Task<IActionResult> Check(string name, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CheckFeatureFlagQuery(name), cancellationToken);

        return Ok(new
        {
            result.Name,
            result.IsEnabled,
            result.Source
        });
    }

    [HttpPut("{name}")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> Toggle(string name, [FromBody] ToggleRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ToggleFeatureFlagCommand(name, request.IsEnabled), cancellationToken);

        return Ok(new { ok = true });
    }

    public sealed record ToggleRequest(bool IsEnabled);
}
