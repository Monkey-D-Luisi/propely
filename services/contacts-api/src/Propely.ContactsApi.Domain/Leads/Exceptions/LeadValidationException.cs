// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Domain.Common.Exceptions;

namespace Propely.ContactsApi.Domain.Leads.Exceptions;

public sealed class LeadValidationException : DomainException
{
    public LeadValidationException(string message) : base(message) { }
    public LeadValidationException(string message, Exception innerException) : base(message, innerException) { }
}
