// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AiApi.Application.Actions.Parameters;

namespace Propely.AiApi.Application.Actions.Interfaces;

/// <summary>
/// Converts untyped action parameter dictionaries into strongly-typed parameter records.
/// </summary>
public interface IParameterBinder
{
    /// <summary>
    /// Binds a raw parameter dictionary (as received from the intent classifier)
    /// to a strongly-typed parameter record.
    /// </summary>
    /// <typeparam name="T">The typed parameter record type.</typeparam>
    /// <param name="raw">The raw parameter dictionary with snake_case keys.</param>
    /// <returns>A typed parameter record with values mapped from the dictionary.</returns>
    T Bind<T>(Dictionary<string, object?> raw) where T : IActionParameters;
}
