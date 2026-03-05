// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { useCallback, useState } from 'react';
import { getAccessToken, setAccessToken } from '@/lib/token-store';
import { getActiveOrgId } from '@/lib/org-store';

const AI_API_BASE =
  process.env.NEXT_PUBLIC_AI_API_URL ?? 'http://localhost:5010';
const API_BASE = process.env.NEXT_PUBLIC_ORGS_API_URL ?? 'http://localhost:5020';

export interface VoiceExecuteResult {
  /** The transcribed text from the audio. */
  transcription: string;
  /** The action result if an action was matched and executed. */
  actionResult?: {
    actionType: string;
    success: boolean;
    message: string;
    data?: unknown;
  };
  /** Natural language confirmation text. */
  confirmationText?: string;
}

export interface UseExecuteVoiceActionReturn {
  executeVoice: (audioBlob: Blob, language?: string) => Promise<VoiceExecuteResult | null>;
  isLoading: boolean;
  result: VoiceExecuteResult | null;
  error: string | null;
}

async function tryRefreshToken(): Promise<boolean> {
  try {
    const res = await fetch(`${API_BASE}/auth/refresh`, {
      method: 'POST',
      credentials: 'include',
      headers: { 'content-type': 'application/json' },
    });
    if (!res.ok) return false;
    const data = await res.json();
    if (data.accessToken) {
      setAccessToken(data.accessToken);
      return true;
    }
    return false;
  } catch {
    return false;
  }
}

async function postVoice(
  audioBlob: Blob,
  language?: string,
): Promise<VoiceExecuteResult> {
  const formData = new FormData();
  formData.append('audio', audioBlob, 'recording.webm');
  if (language) {
    formData.append('language', language);
  }

  const headers: Record<string, string> = {};
  const token = getAccessToken();
  if (token) {
    headers['authorization'] = `Bearer ${token}`;
  }
  const orgId = getActiveOrgId();
  if (orgId) {
    headers['x-org-id'] = orgId;
  }

  const res = await fetch(`${AI_API_BASE}/v1/voice/execute`, {
    method: 'POST',
    credentials: 'include',
    headers,
    body: formData,
  });

  if (!res.ok) {
    if (res.status === 401) {
      const refreshed = await tryRefreshToken();
      if (refreshed) {
        // Retry with new token
        const newHeaders: Record<string, string> = {};
        const newToken = getAccessToken();
        if (newToken) {
          newHeaders['authorization'] = `Bearer ${newToken}`;
        }
        if (orgId) {
          newHeaders['x-org-id'] = orgId;
        }

        const retryRes = await fetch(`${AI_API_BASE}/v1/voice/execute`, {
          method: 'POST',
          credentials: 'include',
          headers: newHeaders,
          body: formData,
        });

        if (!retryRes.ok) {
          throw new Error(`HTTP ${retryRes.status}`);
        }

        return retryRes.json();
      }
    }
    throw new Error(`HTTP ${res.status}`);
  }

  return res.json();
}

/**
 * Hook for executing voice actions via the AI API.
 * Sends audio as multipart/form-data to POST /v1/voice/execute.
 */
export function useExecuteVoiceAction(): UseExecuteVoiceActionReturn {
  const [isLoading, setIsLoading] = useState(false);
  const [result, setResult] = useState<VoiceExecuteResult | null>(null);
  const [error, setError] = useState<string | null>(null);

  const executeVoice = useCallback(
    async (
      audioBlob: Blob,
      language?: string,
    ): Promise<VoiceExecuteResult | null> => {
      setIsLoading(true);
      setError(null);
      setResult(null);

      try {
        const response = await postVoice(audioBlob, language);
        setResult(response);
        return response;
      } catch {
        setError('transcriptionFailed');
        return null;
      } finally {
        setIsLoading(false);
      }
    },
    [],
  );

  return { executeVoice, isLoading, result, error };
}
