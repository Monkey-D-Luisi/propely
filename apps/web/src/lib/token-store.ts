// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

/**
 * In-memory access token store for cross-origin API calls.
 *
 * When the web frontend and AI API live on different origins (e.g. separate
 * Cloud Run services on .run.app), HttpOnly cookies scoped to orgs-api cannot
 * be sent to ai-api. This module stores the JWT access token in memory so it
 * can be attached as an Authorization: Bearer header instead.
 *
 * The token is never persisted to localStorage/sessionStorage for security.
 * On page reload the token is re-obtained via /auth/refresh.
 */

let accessToken: string | null = null;

export function getAccessToken(): string | null {
  return accessToken;
}

export function setAccessToken(token: string | null): void {
  accessToken = token;
}

export function clearAccessToken(): void {
  accessToken = null;
}
