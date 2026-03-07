// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, expect, it, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';

vi.mock('@/lib/api', () => ({
  contactsApiFetch: vi.fn(),
}));

import { contactsApiFetch } from '@/lib/api';
import { useLead } from '../useLead';

const mockFetch = vi.mocked(contactsApiFetch);

describe('useLead', () => {
  beforeEach(() => vi.clearAllMocks());

  it('fetches lead on mount when id is provided', async () => {
    const lead = { id: '1', name: 'Test Lead', status: 'New' };
    mockFetch.mockResolvedValueOnce(lead as never);

    const { result } = renderHook(() => useLead('1'));

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.lead).toEqual(lead);
    expect(mockFetch).toHaveBeenCalledWith(
      '/api/leads/1',
      expect.objectContaining({ method: 'GET' }),
    );
  });

  it('does not fetch when id is null', () => {
    const { result } = renderHook(() => useLead(null));

    expect(result.current.isLoading).toBe(false);
    expect(result.current.lead).toBeNull();
    expect(mockFetch).not.toHaveBeenCalled();
  });

  it('handles fetch error', async () => {
    mockFetch.mockRejectedValueOnce(new Error('Not found'));

    const { result } = renderHook(() => useLead('999'));

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.error).toBeTruthy();
  });
});
