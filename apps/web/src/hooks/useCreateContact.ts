// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useCallback } from 'react';
import { contactsApiFetch } from '@/lib/api';
import type { Contact } from '@/hooks/useContacts';

export function useCreateContact() {
  return useCallback(async (data: Record<string, unknown>) => {
    return contactsApiFetch<Contact>('/api/contacts', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }, []);
}
