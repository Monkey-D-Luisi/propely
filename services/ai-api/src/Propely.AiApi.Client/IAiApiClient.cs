// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Client;

/// <summary>
/// Aggregate interface combining all AI API client capabilities.
/// Consumers can depend on this single interface to access action execution,
/// voice transcription, and content generation features.
/// </summary>
/// <remarks>
/// For most use cases, prefer depending on the specific sub-interfaces
/// (<see cref="IActionApi"/>, <see cref="IVoiceApi"/>, <see cref="IContentApi"/>)
/// to follow the Interface Segregation Principle.
/// </remarks>
public interface IAiApiClient : IActionApi, IVoiceApi, IContentApi
{
}
