// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Domain.WorkItems.Exceptions;

/// <summary>
/// Exception thrown when WorkItem validation fails.
/// </summary>
public sealed class WorkItemValidationException : Exception
{
    public string PropertyName { get; }

    public WorkItemValidationException(string propertyName, string message)
        : base(message)
    {
        PropertyName = propertyName;
    }

    public WorkItemValidationException(string propertyName, string message, Exception innerException)
        : base(message, innerException)
    {
        PropertyName = propertyName;
    }
}
