// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useCallback } from 'react';
import { contactsApiFetch } from '@/lib/api';
import type { Lead, LeadStatus } from '@/hooks/useLeads';

export function useChangeLeadStatus() {
  return useCallback(async (id: string, status: LeadStatus) => {
    return contactsApiFetch<Lead>(`/api/leads/${id}/status`, {
      method: 'PUT',
      body: JSON.stringify({ status }),
    });
  }, []);
}
