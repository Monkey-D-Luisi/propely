// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Client.Dtos;

/// <summary>
/// Request to execute an AI action from natural language text.
/// </summary>
public sealed record ExecuteActionRequest(string Text);
