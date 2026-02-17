// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest';
import { z } from 'zod';
import { apiFetch, ApiError, isApiError, getDomainErrorCode } from '@/lib/api';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------
const mockFetch = vi.fn();

beforeEach(() => {
  mockFetch.mockReset();
  vi.stubGlobal('fetch', mockFetch);
});

afterEach(() => {
  vi.restoreAllMocks();
});

function jsonResponse(body: unknown, status = 200) {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'content-type': 'application/json' },
  });
}

function textResponse(text: string, status = 200) {
  return new Response(text, { status });
}

// ---------------------------------------------------------------------------
// ApiError
// ---------------------------------------------------------------------------
describe('ApiError', () => {
  it('has name, status, and body', () => {
    const err = new ApiError('Not found', 404, { detail: 'missing' }, 30);
    expect(err.name).toBe('ApiError');
    expect(err.message).toBe('Not found');
    expect(err.status).toBe(404);
    expect(err.body).toEqual({ detail: 'missing' });
    expect(err.retryAfterSeconds).toBe(30);
  });

  it('body is optional', () => {
    const err = new ApiError('Fail', 500);
    expect(err.body).toBeUndefined();
    expect(err.retryAfterSeconds).toBeUndefined();
  });
});

describe('isApiError', () => {
  it('returns true for ApiError', () => {
    expect(isApiError(new ApiError('x', 400))).toBe(true);
  });

  it('returns false for generic Error', () => {
    expect(isApiError(new Error('x'))).toBe(false);
  });

  it('returns false for non-error values', () => {
    expect(isApiError(null)).toBe(false);
    expect(isApiError('string')).toBe(false);
  });
});

// ---------------------------------------------------------------------------
// apiFetch — success cases
// ---------------------------------------------------------------------------
describe('apiFetch — success', () => {
  it('returns parsed JSON for successful response', async () => {
    mockFetch.mockResolvedValueOnce(jsonResponse({ data: 'hello' }));

    const result = await apiFetch('/test');
    expect(result).toEqual({ data: 'hello' });
  });

  it('sends credentials and content-type headers', async () => {
    mockFetch.mockResolvedValueOnce(jsonResponse({ ok: true }));

    await apiFetch('/test');

    expect(mockFetch).toHaveBeenCalledWith(
      expect.stringContaining('/test'),
      expect.objectContaining({
        credentials: 'include',
        headers: expect.objectContaining({
          'content-type': 'application/json',
        }),
      }),
    );
  });

  it('merges custom headers', async () => {
    mockFetch.mockResolvedValueOnce(jsonResponse({ ok: true }));

    await apiFetch('/test', { headers: { 'x-custom': 'value' } });

    expect(mockFetch).toHaveBeenCalledWith(
      expect.any(String),
      expect.objectContaining({
        headers: expect.objectContaining({
          'content-type': 'application/json',
          'x-custom': 'value',
        }),
      }),
    );
  });

  it('validates response with schema when provided', async () => {
    const schema = z.object({ count: z.number() });
    mockFetch.mockResolvedValueOnce(jsonResponse({ count: 42 }));

    const result = await apiFetch('/test', {}, schema);
    expect(result).toEqual({ count: 42 });
  });

  it('throws ZodError when schema validation fails', async () => {
    const schema = z.object({ count: z.number() });
    mockFetch.mockResolvedValueOnce(jsonResponse({ count: 'not-a-number' }));

    await expect(apiFetch('/test', {}, schema)).rejects.toThrow();
  });

  it('returns empty object for empty response body', async () => {
    mockFetch.mockResolvedValueOnce(textResponse('', 200));

    const result = await apiFetch('/test');
    expect(result).toEqual({});
  });
});

// ---------------------------------------------------------------------------
// apiFetch — error cases
// ---------------------------------------------------------------------------
describe('apiFetch — errors', () => {
  it('throws ApiError with JSON error message on non-ok response', async () => {
    mockFetch.mockResolvedValueOnce(
      jsonResponse({ error: 'Invalid credentials' }, 401),
    );

    try {
      await apiFetch('/auth/login');
      expect.fail('should have thrown');
    } catch (err) {
      expect(isApiError(err)).toBe(true);
      const apiErr = err as ApiError;
      expect(apiErr.status).toBe(401);
      expect(apiErr.message).toBe('Invalid credentials');
      expect(apiErr.body).toEqual({ error: 'Invalid credentials' });
    }
  });

  it('uses HTTP status as message when JSON has no error field', async () => {
    mockFetch.mockResolvedValueOnce(jsonResponse({ detail: 'something' }, 403));

    try {
      await apiFetch('/test');
      expect.fail('should have thrown');
    } catch (err) {
      const apiErr = err as ApiError;
      expect(apiErr.message).toBe('HTTP 403');
    }
  });

  it('handles non-JSON error response', async () => {
    mockFetch.mockResolvedValueOnce(textResponse('Server Error', 500));

    try {
      await apiFetch('/test');
      expect.fail('should have thrown');
    } catch (err) {
      const apiErr = err as ApiError;
      expect(apiErr.status).toBe(500);
      expect(apiErr.message).toBe('HTTP 500');
      expect(apiErr.body).toBeUndefined();
    }
  });

  it('handles empty body error response', async () => {
    mockFetch.mockResolvedValueOnce(textResponse('', 404));

    try {
      await apiFetch('/test');
      expect.fail('should have thrown');
    } catch (err) {
      const apiErr = err as ApiError;
      expect(apiErr.status).toBe(404);
      expect(apiErr.message).toBe('HTTP 404');
    }
  });

  it('parses Retry-After header for 429 responses', async () => {
    mockFetch.mockResolvedValueOnce(
      new Response(JSON.stringify({ error: 'RATE_LIMIT_EXCEEDED' }), {
        status: 429,
        headers: {
          'content-type': 'application/json',
          'retry-after': '42',
        },
      }),
    );

    try {
      await apiFetch('/auth/login');
      expect.fail('should have thrown');
    } catch (err) {
      const apiErr = err as ApiError;
      expect(apiErr.status).toBe(429);
      expect(apiErr.retryAfterSeconds).toBe(42);
    }
  });

  it('ignores non-numeric Retry-After delta-seconds values', async () => {
    mockFetch.mockResolvedValueOnce(
      new Response(JSON.stringify({ error: 'RATE_LIMIT_EXCEEDED' }), {
        status: 429,
        headers: {
          'content-type': 'application/json',
          'retry-after': '42seconds',
        },
      }),
    );

    try {
      await apiFetch('/auth/login');
      expect.fail('should have thrown');
    } catch (err) {
      const apiErr = err as ApiError;
      expect(apiErr.status).toBe(429);
      expect(apiErr.retryAfterSeconds).toBeUndefined();
    }
  });

  it('parses Retry-After HTTP-date values for 429 responses', async () => {
    const retryAt = new Date(Date.now() + 5_000).toUTCString();
    mockFetch.mockResolvedValueOnce(
      new Response(JSON.stringify({ error: 'RATE_LIMIT_EXCEEDED' }), {
        status: 429,
        headers: {
          'content-type': 'application/json',
          'retry-after': retryAt,
        },
      }),
    );

    try {
      await apiFetch('/auth/login');
      expect.fail('should have thrown');
    } catch (err) {
      const apiErr = err as ApiError;
      expect(apiErr.status).toBe(429);
      expect(apiErr.retryAfterSeconds).toBeGreaterThanOrEqual(0);
      expect(apiErr.retryAfterSeconds).toBeLessThanOrEqual(5);
    }
  });

  it('propagates network errors', async () => {
    mockFetch.mockRejectedValueOnce(new TypeError('Failed to fetch'));

    await expect(apiFetch('/test')).rejects.toThrow('Failed to fetch');
  });
});

// ---------------------------------------------------------------------------
// apiFetch — API_BASE
// ---------------------------------------------------------------------------
describe('apiFetch — base URL', () => {
  it('prefixes path with API base URL', async () => {
    mockFetch.mockResolvedValueOnce(jsonResponse({ ok: true }));

    await apiFetch('/auth/me');

    const calledUrl = mockFetch.mock.calls[0][0] as string;
    expect(calledUrl).toMatch(/\/auth\/me$/);
  });
});

// ---------------------------------------------------------------------------
// getDomainErrorCode
// ---------------------------------------------------------------------------
describe('getDomainErrorCode', () => {
  it('extracts detail from ProblemDetails body', () => {
    const error = new ApiError('Bad Request', 400, { detail: 'CANNOT_REMOVE_LAST_OWNER' });
    expect(getDomainErrorCode(error)).toBe('CANNOT_REMOVE_LAST_OWNER');
  });

  it('returns undefined when body has no detail', () => {
    const error = new ApiError('Bad Request', 400, { error: 'something' });
    expect(getDomainErrorCode(error)).toBeUndefined();
  });

  it('returns undefined when body is undefined', () => {
    const error = new ApiError('Bad Request', 400);
    expect(getDomainErrorCode(error)).toBeUndefined();
  });

  it('returns undefined when detail is not a string', () => {
    const error = new ApiError('Bad Request', 400, { detail: 42 });
    expect(getDomainErrorCode(error)).toBeUndefined();
  });
});
