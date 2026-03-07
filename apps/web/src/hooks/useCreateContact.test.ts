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
import { useCreateContact } from './useCreateContact';

const mockFetch = vi.mocked(contactsApiFetch);

describe('useCreateContact', () => {
  it('returns a callback function', () => {
    const { result } = renderHook(() => useCreateContact());
    expect(typeof result.current).toBe('function');
  });

  it('calls contactsApiFetch with POST method', async () => {
    mockFetch.mockResolvedValueOnce({ id: '1', firstName: 'Test' } as never);

    const { result } = renderHook(() => useCreateContact());
    await act(async () => {
      await result.current({ firstName: 'Test' });
    });

    expect(mockFetch).toHaveBeenCalledWith('/api/contacts', {
      method: 'POST',
      headers: { 'x-csrf-token': 'mock-csrf-token' },
      body: JSON.stringify({ firstName: 'Test' }),
    });
  });
});
