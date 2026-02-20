// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Infrastructure.Services;

public class OpenAiOptions
{
    public const string SectionName = "OpenAi";

    public string ModelId { get; set; } = "gpt-5-mini";

    public string? ApiKey { get; set; }
}
