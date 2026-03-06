// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useCallback } from 'react';
import { appointmentsApiFetch } from '@/lib/api';

export function useDeleteAppointment() {
  return useCallback(async (id: string) => {
    return appointmentsApiFetch<void>(`/api/appointments/${id}`, {
      method: 'DELETE',
    });
  }, []);
}
