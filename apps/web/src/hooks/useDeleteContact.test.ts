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
import { useDeleteContact } from './useDeleteContact';

const mockFetch = vi.mocked(contactsApiFetch);

describe('useDeleteContact', () => {
  it('returns a callback function', () => {
    const { result } = renderHook(() => useDeleteContact());
    expect(typeof result.current).toBe('function');
  });

  it('calls contactsApiFetch with DELETE method', async () => {
    mockFetch.mockResolvedValueOnce(undefined as never);

    const { result } = renderHook(() => useDeleteContact());
    await act(async () => {
      await result.current('contact-123');
    });

    expect(mockFetch).toHaveBeenCalledWith('/api/contacts/contact-123', {
      method: 'DELETE',
      headers: { 'x-csrf-token': 'mock-csrf-token' },
    });
  });
});
