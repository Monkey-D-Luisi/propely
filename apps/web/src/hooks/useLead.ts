// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useEffect, useCallback } from 'react';
import { contactsApiFetch } from '@/lib/api';
import type { Lead } from '@/hooks/useLeads';

export function useLead(id: string | null) {
  const [lead, setLead] = useState<Lead | null>(null);
  const [isLoading, setLoading] = useState(false);
  const [error, setError] = useState<unknown>(null);

  const fetcher = useCallback(async (signal?: AbortSignal) => {
    if (!id) return;
    setLoading(true);
    setError(null);
    try {
      const data = await contactsApiFetch<Lead>(
        `/api/leads/${id}`,
        { method: 'GET', signal },
      );
      setLead(data);
    } catch (e) {
      if (e instanceof DOMException && e.name === 'AbortError') return;
      setError(e);
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    if (!id) {
      setLead(null);
      return;
    }
    const controller = new AbortController();
    void fetcher(controller.signal);
    return () => { controller.abort(); };
  }, [fetcher, id]);

  return { lead, isLoading, error, refetch: fetcher };
}
