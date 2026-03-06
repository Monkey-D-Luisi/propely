// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useCallback } from 'react';
import { appointmentsApiFetch } from '@/lib/api';
import type { Appointment, AppointmentStatus } from '@/hooks/useAppointments';

const statusEndpointMap: Record<string, string> = {
  Confirmed: 'confirm',
  Completed: 'complete',
  Cancelled: 'cancel',
  NoShow: 'no-show',
};

export function useUpdateAppointmentStatus() {
  return useCallback(async (id: string, status: AppointmentStatus, cancellationReason?: string) => {
    const endpoint = statusEndpointMap[status];
    if (!endpoint) {
      throw new Error(`No status transition endpoint for status '${status}'`);
    }

    const body: Record<string, string> | undefined =
      status === 'Cancelled' && cancellationReason ? { reason: cancellationReason } :
      status === 'Completed' ? {} :
      undefined;

    return appointmentsApiFetch<Appointment>(`/api/appointments/${id}/${endpoint}`, {
      method: 'PUT',
      ...(body !== undefined && { body: JSON.stringify(body) }),
    });
  }, []);
}
