// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System.Threading;
using System.Threading.Tasks;

namespace SaasTemplate.AiApi.Application.Common.Interfaces;

public interface IOpenAiService
{
    Task<string> GenerateTextAsync(string prompt, CancellationToken cancellationToken = default);
}
