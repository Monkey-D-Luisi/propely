// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useCallback } from 'react';
import { appointmentsApiFetch } from '@/lib/api';
import type { Appointment, AppointmentStatus } from '@/hooks/useAppointments';

export function useUpdateAppointmentStatus() {
  return useCallback(async (id: string, status: AppointmentStatus, cancellationReason?: string) => {
    return appointmentsApiFetch<Appointment>(`/api/appointments/${id}/status`, {
      method: 'PUT',
      body: JSON.stringify({ status, cancellationReason }),
    });
  }, []);
}
