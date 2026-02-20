// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.PublishingApi.Domain.Common.Exceptions;

public sealed class TenantMismatchException : ForbiddenException
{
    public TenantMismatchException(string message) : base(message) { }
    public TenantMismatchException(string message, Exception innerException) : base(message, innerException) { }
    public TenantMismatchException() { }
}
