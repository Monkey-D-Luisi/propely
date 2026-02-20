// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Api.Dtos;

public sealed record PaymentRequest(Guid OrgId, long Amount, string Currency, string Description, string SuccessUrl, string CancelUrl);
