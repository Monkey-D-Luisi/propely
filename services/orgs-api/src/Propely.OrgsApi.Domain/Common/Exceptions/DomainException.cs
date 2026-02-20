// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Domain.Common.Exceptions;

/// <summary>
/// Base exception for all domain-level errors.
/// Maps to HTTP 400 Bad Request by default.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public DomainException()
    {
    }
}
