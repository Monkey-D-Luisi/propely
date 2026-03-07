// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useCallback } from 'react';
import { contactsApiFetch } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';
import type { Contact } from '@/hooks/useContacts';

export function useUpdateContact() {
  return useCallback(async (id: string, data: Record<string, unknown>) => {
    const csrfToken = await ensureCsrfToken();
    return contactsApiFetch<Contact>(`/api/contacts/${id}`, {
      method: 'PUT',
      headers: csrfToken ? { 'x-csrf-token': csrfToken } : {},
      body: JSON.stringify(data),
    });
  }, []);
}
