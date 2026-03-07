// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useCallback } from 'react';
import { appointmentsApiFetch } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';
import type { Appointment } from '@/hooks/useAppointments';

export interface CreateAppointmentRequest {
  title: string;
  type: string;
  startTimeUtc: string;
  endTimeUtc: string;
  description?: string | null;
  location?: string | null;
  isAllDay?: boolean;
  propertyId?: string | null;
  contactId?: string | null;
  notes?: string | null;
}

export function useCreateAppointment() {
  return useCallback(async (data: CreateAppointmentRequest) => {
    const csrfToken = await ensureCsrfToken();
    return appointmentsApiFetch<Appointment>('/api/appointments', {
      method: 'POST',
      headers: csrfToken ? { 'x-csrf-token': csrfToken } : {},
      body: JSON.stringify(data),
    });
  }, []);
}
