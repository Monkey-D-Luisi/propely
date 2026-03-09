// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useState, useCallback } from 'react';
import { aiApiFetch } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';

export interface ActionResult {
  success: boolean;
  message: string;
  actionType?: string;
  needsConfirmation?: boolean;
  extractedParams?: Record<string, unknown>;
  confirmationId?: string;
  errors?: string[];
  confidence?: number;
}

export function useExecuteAction() {
  const [isLoading, setIsLoading] = useState(false);
  const [result, setResult] = useState<ActionResult | null>(null);
  const [error, setError] = useState<string | null>(null);

  const execute = useCallback(async (text: string, sessionId?: string) => {
    setIsLoading(true);
    setError(null);
    setResult(null);
    try {
      const csrfToken = await ensureCsrfToken();
      const data = await aiApiFetch<ActionResult>('/v1/actions/execute', {
        method: 'POST',
        headers: csrfToken ? { 'x-csrf-token': csrfToken } : {},
        body: JSON.stringify({ text, sessionId }),
      });
      setResult(data);
      return data;
    } catch (e) {
      const message = e instanceof Error ? e.message : 'Unknown error';
      setError(message);
      return null;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const confirm = useCallback(async (confirmationId: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const csrfToken = await ensureCsrfToken();
      const data = await aiApiFetch<ActionResult>('/v1/actions/confirm', {
        method: 'POST',
        headers: csrfToken ? { 'x-csrf-token': csrfToken } : {},
        body: JSON.stringify({ confirmationId }),
      });
      setResult(data);
      return data;
    } catch (e) {
      const message = e instanceof Error ? e.message : 'Unknown error';
      setError(message);
      return null;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const reset = useCallback(() => {
    setIsLoading(false);
    setResult(null);
    setError(null);
  }, []);

  return { execute, confirm, isLoading, result, error, reset, setResult };
}
