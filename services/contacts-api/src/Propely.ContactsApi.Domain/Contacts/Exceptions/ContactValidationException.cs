// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.ContactsApi.Domain.Common.Exceptions;

namespace Propely.ContactsApi.Domain.Contacts.Exceptions;

public sealed class ContactValidationException : DomainException
{
    public ContactValidationException(string message) : base(message) { }
    public ContactValidationException(string message, Exception innerException) : base(message, innerException) { }
}
