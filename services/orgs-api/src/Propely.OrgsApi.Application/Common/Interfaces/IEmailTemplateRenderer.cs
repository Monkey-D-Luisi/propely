// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.OrgsApi.Application.Common.Email;

namespace Propely.OrgsApi.Application.Common.Interfaces;

public interface IEmailTemplateRenderer
{
    Task<string> RenderAsync<TModel>(
        string templateName,
        TModel model,
        string locale = "en",
        CancellationToken cancellationToken = default)
        where TModel : BaseEmailModel;
}
