// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Domain.Common.Exceptions;

/// <summary>
/// Exception thrown when an operation attempts to access or modify
/// data belonging to a different tenant than the current context.
/// Maps to HTTP 403 Forbidden via ForbiddenException.
/// </summary>
public sealed class TenantMismatchException : ForbiddenException
{
    public TenantMismatchException(string message) : base(message) { }
    public TenantMismatchException(string message, Exception innerException) : base(message, innerException) { }
    public TenantMismatchException() { }
}
