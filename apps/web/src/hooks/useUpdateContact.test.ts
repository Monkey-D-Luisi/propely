// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, expect, it, vi } from 'vitest';
import { renderHook, act } from '@testing-library/react';

vi.mock('@/lib/api', () => ({
  contactsApiFetch: vi.fn(),
}));

import { contactsApiFetch } from '@/lib/api';
import { useUpdateContact } from './useUpdateContact';

const mockFetch = vi.mocked(contactsApiFetch);

describe('useUpdateContact', () => {
  it('returns a callback function', () => {
    const { result } = renderHook(() => useUpdateContact());
    expect(typeof result.current).toBe('function');
  });

  it('calls contactsApiFetch with PUT method', async () => {
    mockFetch.mockResolvedValueOnce({ id: '1', firstName: 'Updated' } as never);

    const { result } = renderHook(() => useUpdateContact());
    await act(async () => {
      await result.current('contact-1', { firstName: 'Updated' });
    });

    expect(mockFetch).toHaveBeenCalledWith('/api/contacts/contact-1', {
      method: 'PUT',
      body: JSON.stringify({ firstName: 'Updated' }),
    });
  });
});
