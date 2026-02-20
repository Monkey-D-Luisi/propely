// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Domain.Common.Exceptions;

/// <summary>
/// Exception thrown when an external AI service call fails.
/// Maps to HTTP 502 Bad Gateway by default.
/// </summary>
public sealed class AiServiceException : Exception
{
    public AiServiceException(string message) : base(message)
    {
    }

    public AiServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public AiServiceException()
    {
    }
}
