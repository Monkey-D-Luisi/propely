// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import type { AppointmentStatus } from '@/hooks/useAppointments';

const statusConfig: Record<AppointmentStatus, { bg: string; text: string }> = {
  Scheduled: { bg: 'bg-blue-100', text: 'text-blue-700' },
  Confirmed: { bg: 'bg-green-100', text: 'text-green-700' },
  Completed: { bg: 'bg-slate-100', text: 'text-slate-700' },
  Cancelled: { bg: 'bg-red-100', text: 'text-red-700' },
  NoShow: { bg: 'bg-amber-100', text: 'text-amber-700' },
};

interface AppointmentStatusBadgeProps {
  status: AppointmentStatus;
  label: string;
}

export function AppointmentStatusBadge({ status, label }: AppointmentStatusBadgeProps) {
  const config = statusConfig[status] ?? statusConfig.Scheduled;
  return (
    <span
      data-testid={`appointment-status-badge-${status}`}
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${config.bg} ${config.text}`}
    >
      {label}
    </span>
  );
}
