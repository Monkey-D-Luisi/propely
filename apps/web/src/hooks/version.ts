// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useEffect, useCallback } from 'react';
import { apiFetch } from '@/lib/api';
import {
  VersionResponseSchema,
  UpdateCheckResponseSchema,
} from '@/lib/schemas';
import type { VersionResponse, UpdateCheckResponse } from '@/lib/schemas';

export function useVersion() {
  const [version, setVersion] = useState<VersionResponse | null>(null);
  const [isLoading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  const fetcher = useCallback(async (signal?: AbortSignal) => {
    setLoading(true);
    setError(null);
    try {
      const data = await apiFetch<VersionResponse>(
        '/version',
        { method: 'GET', signal },
        VersionResponseSchema,
      );
      setVersion(data);
    } catch (e) {
      if (e instanceof DOMException && e.name === 'AbortError') return;
      setError(e);
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    void fetcher(controller.signal);
    return () => { controller.abort(); };
  }, [fetcher]);

  return { version, isLoading, error, refetch: fetcher };
}

export function useUpdateCheck() {
  const [updateInfo, setUpdateInfo] = useState<UpdateCheckResponse | null>(null);
  const [isChecking, setChecking] = useState(false);
  const [error, setError] = useState<unknown>(null);

  const checkForUpdates = useCallback(async () => {
    setChecking(true);
    setError(null);
    try {
      const data = await apiFetch<UpdateCheckResponse>(
        '/version/check',
        { method: 'GET' },
        UpdateCheckResponseSchema,
      );
      setUpdateInfo(data);
    } catch (e) {
      setError(e);
    } finally {
      setChecking(false);
    }
  }, []);

  return { updateInfo, isChecking, error, checkForUpdates };
}
