// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { getAccessToken, setAccessToken } from './token-store';
import { getActiveOrgId } from './org-store';

export class ApiError extends Error {
  status: number;
  body?: unknown;
  retryAfterSeconds?: number;
  constructor(message: string, status: number, body?: unknown, retryAfterSeconds?: number) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.body = body;
    this.retryAfterSeconds = retryAfterSeconds;
  }
}
export const isApiError = (e: unknown): e is ApiError => e instanceof ApiError;

/** Extract the ProblemDetails `detail` field from an ApiError body. */
export function getDomainErrorCode(error: ApiError): string | undefined {
  if (error.body && typeof error.body === 'object' && 'detail' in error.body) {
    const detail = (error.body as { detail?: unknown }).detail;
    return typeof detail === 'string' ? detail : undefined;
  }
  return undefined;
}

const API_BASE = process.env.NEXT_PUBLIC_ORGS_API_URL ?? "http://localhost:5020";
const AI_API_BASE = process.env.NEXT_PUBLIC_AI_API_URL ?? "http://localhost:5010";
const PROPERTIES_API_BASE = process.env.NEXT_PUBLIC_PROPERTIES_API_URL ?? "http://localhost:5030";

const CROSS_ORIGIN_BASES = new Set([AI_API_BASE, PROPERTIES_API_BASE]);

async function baseFetch<T>(
  baseUrl: string,
  path: string,
  init: RequestInit = {},
  schema?: { parse: (data: unknown) => T },
): Promise<T> {
  const headers: Record<string, string> = {
    "content-type": "application/json",
    ...(init.headers as Record<string, string> || {}),
  };

  // Attach Bearer token and org context for cross-origin API calls
  if (CROSS_ORIGIN_BASES.has(baseUrl)) {
    const token = getAccessToken();
    if (token) {
      headers["authorization"] = `Bearer ${token}`;
    }
    const orgId = getActiveOrgId();
    if (orgId) {
      headers["x-org-id"] = orgId;
    }
  }

  const res = await fetch(`${baseUrl}${path}`, {
    ...init,
    credentials: "include",
    headers,
  });

  const text = await res.text();
  const json = text ? safeJson(text) : null;

  if (!res.ok) {
    const retryAfterSeconds = parseRetryAfterSeconds(res.headers.get("retry-after"));
    const msg =
      (json && typeof json === "object" && (json as Record<string, unknown>).error) ||
      `HTTP ${res.status}`;
    throw new ApiError(String(msg), res.status, json ?? undefined, retryAfterSeconds);
  }

  const data = (json ?? ({} as unknown)) as unknown;
  return schema ? schema.parse(data) : (data as T);
}

export async function apiFetch<T>(
  path: string,
  init: RequestInit = {},
  schema?: { parse: (data: unknown) => T },
): Promise<T> {
  return baseFetch(API_BASE, path, init, schema);
}

/**
 * Fetch from the AI API with automatic token refresh on 401.
 *
 * When cookies cannot be shared cross-origin (e.g. .run.app PSL domains),
 * this function uses a Bearer token from the in-memory store. If the AI API
 * returns 401, it attempts a single token refresh via orgs-api and retries.
 */
export async function aiApiFetch<T>(
  path: string,
  init: RequestInit = {},
  schema?: { parse: (data: unknown) => T },
): Promise<T> {
  try {
    return await baseFetch(AI_API_BASE, path, init, schema);
  } catch (err) {
    if (isApiError(err) && err.status === 401) {
      const refreshed = await tryRefreshToken();
      if (refreshed) {
        return await baseFetch(AI_API_BASE, path, init, schema);
      }
    }
    throw err;
  }
}

/**
 * Fetch from the Properties API with automatic token refresh on 401.
 */
export async function propertiesApiFetch<T>(
  path: string,
  init: RequestInit = {},
  schema?: { parse: (data: unknown) => T },
): Promise<T> {
  try {
    return await baseFetch(PROPERTIES_API_BASE, path, init, schema);
  } catch (err) {
    if (isApiError(err) && err.status === 401) {
      const refreshed = await tryRefreshToken();
      if (refreshed) {
        return await baseFetch(PROPERTIES_API_BASE, path, init, schema);
      }
    }
    throw err;
  }
}

// Deduplication: only one refresh in-flight at a time
let refreshPromise: Promise<boolean> | null = null;

export async function tryRefreshToken(): Promise<boolean> {
  if (refreshPromise) return refreshPromise;
  refreshPromise = doRefresh();
  try {
    return await refreshPromise;
  } finally {
    refreshPromise = null;
  }
}

async function doRefresh(): Promise<boolean> {
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

function safeJson(txt: string) {
  try {
    return JSON.parse(txt);
  } catch {
    return null;
  }
}

function parseRetryAfterSeconds(value: string | null): number | undefined {
  if (!value) {
    return undefined;
  }

  if (/^\d+$/.test(value)) {
    const asSeconds = Number(value);
    if (Number.isSafeInteger(asSeconds) && asSeconds >= 0) {
      return asSeconds;
    }
  }

  const asDate = Date.parse(value);
  if (!Number.isNaN(asDate)) {
    const deltaSeconds = Math.ceil((asDate - Date.now()) / 1000);
    return deltaSeconds > 0 ? deltaSeconds : 0;
  }

  return undefined;
}
