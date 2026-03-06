// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useEffect, useCallback } from 'react';
import { contactsApiFetch } from '@/lib/api';
import type { Contact } from '@/hooks/useContacts';

export function useContact(id: string) {
  const [contact, setContact] = useState<Contact | null>(null);
  const [isLoading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  const fetcher = useCallback(async (signal?: AbortSignal) => {
    setLoading(true);
    setError(null);
    try {
      const data = await contactsApiFetch<Contact>(
        `/api/contacts/${id}`,
        { method: 'GET', signal },
      );
      setContact(data);
    } catch (e) {
      if (e instanceof DOMException && e.name === 'AbortError') return;
      setError(e);
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    const controller = new AbortController();
    void fetcher(controller.signal);
    return () => { controller.abort(); };
  }, [fetcher]);

  return { contact, isLoading, error, refetch: fetcher };
}
