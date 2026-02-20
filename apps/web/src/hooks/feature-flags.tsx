// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import { apiFetch } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';
import { FeatureFlagsResponseSchema } from '@/lib/schemas';
import type { FeatureFlag } from '@/lib/schemas';

type FeatureFlagContextValue = {
  flags: FeatureFlag[];
  isLoading: boolean;
  error: unknown;
  refetch: () => Promise<void>;
};

const FeatureFlagContext = createContext<FeatureFlagContextValue | null>(null);

export function FeatureFlagProvider({ children }: { children: ReactNode }) {
  const [flags, setFlags] = useState<FeatureFlag[]>([]);
  const [isLoading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  const fetcher = useCallback(async (signal?: AbortSignal) => {
    try {
      setLoading(true);
      const data = await apiFetch(
        '/feature-flags',
        { method: 'GET', signal },
        FeatureFlagsResponseSchema,
      );
      setFlags(data.flags);
      setError(null);
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

  const value = useMemo(
    () => ({ flags, isLoading, error, refetch: fetcher }),
    [flags, isLoading, error, fetcher],
  );

  return (
    <FeatureFlagContext.Provider value={value}>
      {children}
    </FeatureFlagContext.Provider>
  );
}

export function useFeatureFlags() {
  const ctx = useContext(FeatureFlagContext);
  if (!ctx)
    throw new Error(
      'useFeatureFlags must be used within <FeatureFlagProvider>',
    );
  return ctx;
}

export function useFeatureFlag(name: string) {
  const { flags, isLoading, error } = useFeatureFlags();

  const flag = flags.find(
    (f) => f.name.toLowerCase() === name.toLowerCase(),
  );

  return {
    isEnabled: flag?.isEnabled ?? false,
    isLoading,
    error,
  };
}

export function useToggleFeatureFlag() {
  return useCallback(async (name: string, isEnabled: boolean) => {
    const csrfToken = await ensureCsrfToken();
    await apiFetch(`/feature-flags/${encodeURIComponent(name)}`, {
      method: 'PUT',
      headers: csrfToken ? { 'x-csrf-token': csrfToken } : {},
      body: JSON.stringify({ isEnabled }),
    });
  }, []);
}
