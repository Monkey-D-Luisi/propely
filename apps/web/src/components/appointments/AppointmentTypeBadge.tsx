// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import type { AppointmentType } from '@/hooks/useAppointments';

const typeConfig: Record<AppointmentType, { bg: string; text: string }> = {
  PropertyViewing: { bg: 'bg-indigo-100', text: 'text-indigo-700' },
  OwnerMeeting: { bg: 'bg-amber-100', text: 'text-amber-700' },
  Generic: { bg: 'bg-slate-100', text: 'text-slate-700' },
};

interface AppointmentTypeBadgeProps {
  type: AppointmentType;
  label: string;
}

export function AppointmentTypeBadge({ type, label }: AppointmentTypeBadgeProps) {
  const config = typeConfig[type] ?? typeConfig.Generic;
  return (
    <span
      data-testid={`appointment-type-badge-${type}`}
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${config.bg} ${config.text}`}
    >
      {label}
    </span>
  );
}
