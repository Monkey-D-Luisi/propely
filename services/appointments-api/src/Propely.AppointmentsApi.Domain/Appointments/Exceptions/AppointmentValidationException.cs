// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Propely.AppointmentsApi.Domain.Common.Exceptions;

namespace Propely.AppointmentsApi.Domain.Appointments.Exceptions;

public sealed class AppointmentValidationException : DomainException
{
    public AppointmentValidationException(string message) : base(message) { }
    public AppointmentValidationException(string message, Exception innerException) : base(message, innerException) { }
}
