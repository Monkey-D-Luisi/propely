// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useCallback } from 'react';
import { contactsApiFetch } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';
import type { Contact } from '@/hooks/useContacts';

export function useCreateContact() {
  return useCallback(async (data: Record<string, unknown>) => {
    const csrfToken = await ensureCsrfToken();
    return contactsApiFetch<Contact>('/api/contacts', {
      method: 'POST',
      headers: csrfToken ? { 'x-csrf-token': csrfToken } : {},
      body: JSON.stringify(data),
    });
  }, []);
}
