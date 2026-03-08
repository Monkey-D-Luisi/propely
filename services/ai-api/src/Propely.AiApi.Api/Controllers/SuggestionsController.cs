// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Propely.AiApi.Api.Configuration;
using Propely.AiApi.Application.Suggestions.Queries.GetSuggestions;

namespace Propely.AiApi.Api.Controllers;

[ApiController]
[Route("v1/suggestions")]
[Authorize(Policy = AuthorizationPolicies.RequireViewer)]
public sealed class SuggestionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SuggestionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetSuggestions(CancellationToken cancellationToken)
    {
        if (!User.TryGetOrgId(out var orgId))
            return Forbid();

        var query = new GetSuggestionsQuery(orgId);
        var suggestions = await _mediator.Send(query, cancellationToken);
        return Ok(suggestions);
    }
}
