// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useToggleFeatureFlag } from '@/hooks/feature-flags';

vi.mock('@/lib/api', async () => {
  const actual = await vi.importActual<typeof import('@/lib/api')>('@/lib/api');
  return {
    ...actual,
    apiFetch: vi.fn(),
  };
});

vi.mock('@/lib/csrf', () => ({
  ensureCsrfToken: vi.fn(),
}));

import { apiFetch } from '@/lib/api';
import { ensureCsrfToken } from '@/lib/csrf';

const mockApiFetch = vi.mocked(apiFetch);
const mockEnsureCsrfToken = vi.mocked(ensureCsrfToken);

beforeEach(() => {
  vi.resetAllMocks();
});

describe('useToggleFeatureFlag', () => {
  it('calls PUT with flag name, isEnabled, and CSRF token', async () => {
    mockEnsureCsrfToken.mockResolvedValue('csrf-abc');
    mockApiFetch.mockResolvedValue({ ok: true });

    const { result } = renderHook(() => useToggleFeatureFlag());

    await act(async () => {
      await result.current('DarkMode', true);
    });

    expect(mockApiFetch).toHaveBeenCalledWith(
      '/feature-flags/DarkMode',
      expect.objectContaining({
        method: 'PUT',
        headers: { 'x-csrf-token': 'csrf-abc' },
        body: JSON.stringify({ isEnabled: true }),
      }),
    );
  });

  it('encodes flag name in URL', async () => {
    mockEnsureCsrfToken.mockResolvedValue('csrf-abc');
    mockApiFetch.mockResolvedValue({ ok: true });

    const { result } = renderHook(() => useToggleFeatureFlag());

    await act(async () => {
      await result.current('My Feature', false);
    });

    expect(mockApiFetch).toHaveBeenCalledWith(
      '/feature-flags/My%20Feature',
      expect.objectContaining({
        method: 'PUT',
        body: JSON.stringify({ isEnabled: false }),
      }),
    );
  });

  it('sends empty headers when CSRF token is null', async () => {
    mockEnsureCsrfToken.mockResolvedValue(null);
    mockApiFetch.mockResolvedValue({ ok: true });

    const { result } = renderHook(() => useToggleFeatureFlag());

    await act(async () => {
      await result.current('BetaFeatures', true);
    });

    expect(mockApiFetch).toHaveBeenCalledWith(
      '/feature-flags/BetaFeatures',
      expect.objectContaining({
        method: 'PUT',
        headers: {},
        body: JSON.stringify({ isEnabled: true }),
      }),
    );
  });

  it('propagates API errors', async () => {
    mockEnsureCsrfToken.mockResolvedValue('csrf-abc');
    mockApiFetch.mockRejectedValue(new Error('Network error'));

    const { result } = renderHook(() => useToggleFeatureFlag());

    let error: Error | undefined;
    try {
      await act(async () => {
        await result.current('DarkMode', true);
      });
    } catch (e) {
      error = e as Error;
    }

    expect(error).toBeDefined();
    expect(error?.message).toBe('Network error');
  });
});
