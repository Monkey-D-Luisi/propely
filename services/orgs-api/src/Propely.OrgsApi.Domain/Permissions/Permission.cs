// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.OrgsApi.Domain.Permissions;

/// <summary>
/// Granular permissions that can be evaluated against a user's role and overrides.
/// </summary>
public enum Permission
{
    PropertiesViewAll,
    PropertiesEditAll,
    ContactsViewAll,
    ContactsEditAll,
    AppointmentsViewAll,
    PublishingManage,
    LeadsManage,
    ReportsView
}
