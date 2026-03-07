// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useCallback } from 'react';
import { contactsApiFetch } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';
import type { ContactRole } from '@/hooks/useContacts';

export interface ConvertLeadResponse {
  lead: {
    id: string;
    name: string;
    status: string;
  };
  contact: {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
  };
  wasNewContact: boolean;
}

export function useConvertLead() {
  return useCallback(async (id: string, role: ContactRole, notes?: string) => {
    const csrfToken = await ensureCsrfToken();
    return contactsApiFetch<ConvertLeadResponse>(`/api/leads/${id}/convert`, {
      method: 'POST',
      headers: csrfToken ? { 'x-csrf-token': csrfToken } : {},
      body: JSON.stringify({ role, notes }),
    });
  }, []);
}
