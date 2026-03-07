// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, expect, it, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';

vi.mock('@/lib/api', () => ({
  apiFetch: vi.fn(),
}));

import { apiFetch } from '@/lib/api';
import { useVersion, useUpdateCheck } from './version';

const mockApiFetch = vi.mocked(apiFetch);

describe('useVersion', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('fetches version on mount', async () => {
    const versionData = { version: '1.0.0', commit: 'abc123', environment: 'prod' };
    mockApiFetch.mockResolvedValueOnce(versionData);

    const { result } = renderHook(() => useVersion());

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(mockApiFetch).toHaveBeenCalledWith(
      '/version',
      expect.objectContaining({ method: 'GET' }),
      expect.anything(),
    );
  });

  it('handles fetch error', async () => {
    mockApiFetch.mockRejectedValueOnce(new Error('Network error'));

    const { result } = renderHook(() => useVersion());

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.error).toBeTruthy();
  });
});

describe('useUpdateCheck', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('returns checkForUpdates callback', () => {
    const { result } = renderHook(() => useUpdateCheck());
    expect(typeof result.current.checkForUpdates).toBe('function');
    expect(result.current.isChecking).toBe(false);
  });
});
