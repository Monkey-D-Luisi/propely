// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, expect, it, vi } from 'vitest';
import { renderHook, act } from '@testing-library/react';

vi.mock('@/lib/api', () => ({
  contactsApiFetch: vi.fn(),
}));

vi.mock('@/lib/csrf', () => ({
  ensureCsrfToken: vi.fn().mockResolvedValue('mock-csrf-token'),
}));

import { contactsApiFetch } from '@/lib/api';
import { useChangeLeadStatus } from './useChangeLeadStatus';

const mockFetch = vi.mocked(contactsApiFetch);

describe('useChangeLeadStatus', () => {
  it('returns a callback function', () => {
    const { result } = renderHook(() => useChangeLeadStatus());
    expect(typeof result.current).toBe('function');
  });

  it('calls contactsApiFetch with PUT to status endpoint', async () => {
    mockFetch.mockResolvedValueOnce({ id: '1', status: 'Qualified' } as never);

    const { result } = renderHook(() => useChangeLeadStatus());
    await act(async () => {
      await result.current('lead-1', 'Qualified');
    });

    expect(mockFetch).toHaveBeenCalledWith('/api/leads/lead-1/status', {
      method: 'PUT',
      headers: { 'x-csrf-token': 'mock-csrf-token' },
      body: JSON.stringify({ status: 'Qualified' }),
    });
  });
});
