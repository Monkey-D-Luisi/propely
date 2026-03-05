// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

namespace Propely.AiApi.Domain.Actions;

/// <summary>
/// Enumeration of all supported AI action types in the Propely platform.
/// </summary>
public enum ActionType
{
    CreateProperty,
    UpdateProperty,
    QueryProperties,
    ChangePropertyStatus,
    GenerateCopy,
    ExtractFromText,
    ExtractFromPhotos,
    CreateLead,
    CreateContact,
    QualifyLead,
    ConvertLead,
    QueryLeads,
    BookViewing,
    QueryAppointments,
    CancelAppointment,
    RescheduleAppointment,
    ReserveProperty,
    CloseOperation,
    ArchiveProperty,
    ReactivateProperty,
    Unknown
}
