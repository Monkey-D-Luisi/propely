// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useCallback } from 'react';
import { contactsApiFetch } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';
import type { Lead, LeadStatus } from '@/hooks/useLeads';

export function useChangeLeadStatus() {
  return useCallback(async (id: string, status: LeadStatus) => {
    const csrfToken = await ensureCsrfToken();
    return contactsApiFetch<Lead>(`/api/leads/${id}/status`, {
      method: 'PUT',
      headers: csrfToken ? { 'x-csrf-token': csrfToken } : {},
      body: JSON.stringify({ status }),
    });
  }, []);
}
