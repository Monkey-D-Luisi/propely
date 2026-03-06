// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useCallback } from 'react';
import { contactsApiFetch } from '@/lib/api';

export function useDeleteContact() {
  return useCallback(async (id: string) => {
    await contactsApiFetch<void>(`/api/contacts/${id}`, {
      method: 'DELETE',
    });
  }, []);
}
