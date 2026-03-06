// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using Microsoft.AspNetCore.DataProtection;
using Propely.AppointmentsApi.Application.Appointments.Interfaces;

namespace Propely.AppointmentsApi.Infrastructure.Calendar;

public sealed class TokenEncryptionService : ITokenEncryptionService
{
    private const string Purpose = "Propely.AppointmentsApi.CalendarTokens";
    private readonly IDataProtector _protector;

    public TokenEncryptionService(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector(Purpose);
    }

    public string Encrypt(string plainText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText);
        return _protector.Protect(plainText);
    }

    public string Decrypt(string cipherText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cipherText);
        return _protector.Unprotect(cipherText);
    }
}
