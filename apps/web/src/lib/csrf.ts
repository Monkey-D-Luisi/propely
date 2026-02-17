// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

'use client';

import { apiFetch } from '@/lib/api';
import { CsrfResponseSchema } from '@/lib/schemas';

/** Maximum token age the backend accepts (1 hour). Use a smaller window
 *  client-side to avoid sending tokens that are about to expire. */
const MAX_TOKEN_AGE_MS = 55 * 60 * 1000; // 55 minutes

export function readCsrfTokenFromCookie(): string | null {
  if (typeof document === 'undefined') return null;
  const cookies = document.cookie.split(';').map((part) => part.trim());
  const csrfEntry = cookies.find((part) => part.startsWith('csrf_token='));
  if (!csrfEntry) return null;
  const [, value] = csrfEntry.split('=');
  return value ? decodeURIComponent(value) : null;
}

/** Check whether the token's embedded timestamp is still within the
 *  acceptable age window. Returns false for unparseable tokens. */
function isTokenFresh(token: string): boolean {
  const dotIndex = token.indexOf('.');
  if (dotIndex <= 0) return false;
  const hexTimestamp = token.substring(0, dotIndex);
  const unixSeconds = parseInt(hexTimestamp, 16);
  if (Number.isNaN(unixSeconds)) return false;
  const ageMs = Date.now() - unixSeconds * 1000;
  return ageMs >= 0 && ageMs <= MAX_TOKEN_AGE_MS;
}

export async function ensureCsrfToken(): Promise<string | null> {
  const current = readCsrfTokenFromCookie();
  if (current && isTokenFresh(current)) return current;

  // Stale or missing — fetch a fresh token from the API.
  // Clear any stale cookie first so it doesn't pollute future reads.
  if (typeof document !== 'undefined') {
    document.cookie = 'csrf_token=; path=/; max-age=0';
  }

  try {
    const data = await apiFetch('/auth/csrf', { method: 'GET' }, CsrfResponseSchema);
    // Mirror the token as a cookie on the page's domain so subsequent
    // reads from readCsrfTokenFromCookie() work without an API call.
    // max-age=3600 matches the server's 1-hour token window.
    if (data.csrfToken && typeof document !== 'undefined') {
      document.cookie = `csrf_token=${encodeURIComponent(data.csrfToken)}; path=/; SameSite=Lax; max-age=3600`;
    }
    return data.csrfToken;
  } catch {
    return null;
  }
}
