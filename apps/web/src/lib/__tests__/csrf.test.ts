// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest';
import { readCsrfTokenFromCookie, ensureCsrfToken } from '@/lib/csrf';

// Mock apiFetch — must be hoisted before the module loads
vi.mock('@/lib/api', () => ({
  apiFetch: vi.fn(),
  ApiError: class ApiError extends Error {
    status: number;
    body?: unknown;
    constructor(message: string, status: number, body?: unknown) {
      super(message);
      this.name = 'ApiError';
      this.status = status;
      this.body = body;
    }
  },
  isApiError: (e: unknown) => e instanceof Error && e.constructor.name === 'ApiError',
}));

import { apiFetch } from '@/lib/api';
const mockApiFetch = vi.mocked(apiFetch);

// ---------------------------------------------------------------------------
// readCsrfTokenFromCookie
// ---------------------------------------------------------------------------
describe('readCsrfTokenFromCookie', () => {
  afterEach(() => {
    // Reset cookie
    if (typeof document !== 'undefined') {
      Object.defineProperty(document, 'cookie', {
        writable: true,
        value: '',
      });
    }
  });

  it('returns the csrf_token value from cookies', () => {
    Object.defineProperty(document, 'cookie', {
      writable: true,
      value: 'other=123; csrf_token=abc-def; session=xyz',
    });

    expect(readCsrfTokenFromCookie()).toBe('abc-def');
  });

  it('returns null when csrf_token cookie is missing', () => {
    Object.defineProperty(document, 'cookie', {
      writable: true,
      value: 'session=xyz; other=123',
    });

    expect(readCsrfTokenFromCookie()).toBeNull();
  });

  it('decodes URL-encoded token', () => {
    Object.defineProperty(document, 'cookie', {
      writable: true,
      value: 'csrf_token=hello%20world',
    });

    expect(readCsrfTokenFromCookie()).toBe('hello world');
  });

  it('returns null when cookie value is empty', () => {
    Object.defineProperty(document, 'cookie', {
      writable: true,
      value: 'csrf_token=',
    });

    expect(readCsrfTokenFromCookie()).toBeNull();
  });
});

// ---------------------------------------------------------------------------
// ensureCsrfToken
// ---------------------------------------------------------------------------
describe('ensureCsrfToken', () => {
  beforeEach(() => {
    mockApiFetch.mockReset();
  });

  it('returns existing cookie token without making an API call', async () => {
    // Token format: {hex-unix-timestamp}.{random} — must be fresh for isTokenFresh()
    const nowSeconds = Math.floor(Date.now() / 1000);
    const freshToken = `${nowSeconds.toString(16)}.random-suffix`;

    Object.defineProperty(document, 'cookie', {
      writable: true,
      value: `csrf_token=${freshToken}`,
    });

    const result = await ensureCsrfToken();
    expect(result).toBe(freshToken);
    expect(mockApiFetch).not.toHaveBeenCalled();
  });

  it('fetches token from API when cookie is missing', async () => {
    Object.defineProperty(document, 'cookie', {
      writable: true,
      value: '',
    });

    mockApiFetch.mockResolvedValueOnce({ csrfToken: 'new-token' });

    const result = await ensureCsrfToken();
    expect(result).toBe('new-token');
    expect(mockApiFetch).toHaveBeenCalledWith(
      '/auth/csrf',
      { method: 'GET' },
      expect.anything(),
    );
  });

  it('returns null when API call fails', async () => {
    Object.defineProperty(document, 'cookie', {
      writable: true,
      value: '',
    });

    mockApiFetch.mockRejectedValueOnce(new Error('Network error'));

    const result = await ensureCsrfToken();
    expect(result).toBeNull();
  });
});
