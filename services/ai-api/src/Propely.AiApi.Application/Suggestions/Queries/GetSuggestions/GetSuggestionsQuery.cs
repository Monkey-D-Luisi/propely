// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using MediatR;
using Propely.AiApi.Domain.Suggestions;
using Propely.AiApi.Application.Suggestions.Rules;

namespace Propely.AiApi.Application.Suggestions.Queries.GetSuggestions;

public sealed record GetSuggestionsQuery(Guid TenantId) : IRequest<IReadOnlyList<Suggestion>>;

public sealed class GetSuggestionsQueryHandler : IRequestHandler<GetSuggestionsQuery, IReadOnlyList<Suggestion>>
{
    private readonly SuggestionEngine _engine;

    public GetSuggestionsQueryHandler(SuggestionEngine engine)
    {
        _engine = engine;
    }

    public async Task<IReadOnlyList<Suggestion>> Handle(GetSuggestionsQuery request, CancellationToken cancellationToken)
    {
        return await _engine.GenerateAsync(request.TenantId, cancellationToken);
    }
}
