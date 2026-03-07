// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, expect, it, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';

vi.mock('@/lib/api', () => ({
  contactsApiFetch: vi.fn(),
}));

import { contactsApiFetch } from '@/lib/api';
import { useContact } from '../useContact';

const mockFetch = vi.mocked(contactsApiFetch);

describe('useContact', () => {
  beforeEach(() => vi.clearAllMocks());

  it('fetches contact on mount', async () => {
    const contact = { id: '1', firstName: 'John', lastName: 'Doe' };
    mockFetch.mockResolvedValueOnce(contact as never);

    const { result } = renderHook(() => useContact('1'));

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.contact).toEqual(contact);
    expect(mockFetch).toHaveBeenCalledWith(
      '/api/contacts/1',
      expect.objectContaining({ method: 'GET' }),
    );
  });

  it('handles fetch error', async () => {
    mockFetch.mockRejectedValueOnce(new Error('Not found'));

    const { result } = renderHook(() => useContact('999'));

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.error).toBeTruthy();
    expect(result.current.contact).toBeNull();
  });
});
