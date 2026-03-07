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
import { useConvertLead } from './useConvertLead';

const mockFetch = vi.mocked(contactsApiFetch);

describe('useConvertLead', () => {
  it('returns a callback function', () => {
    const { result } = renderHook(() => useConvertLead());
    expect(typeof result.current).toBe('function');
  });

  it('calls contactsApiFetch with POST to convert endpoint', async () => {
    mockFetch.mockResolvedValueOnce({
      lead: { id: '1', name: 'Test', status: 'Converted' },
      contact: { id: '2', firstName: 'Test', lastName: 'User', email: 'test@test.com' },
      wasNewContact: true,
    } as never);

    const { result } = renderHook(() => useConvertLead());
    await act(async () => {
      await result.current('lead-1', 'Buyer', 'some notes');
    });

    expect(mockFetch).toHaveBeenCalledWith('/api/leads/lead-1/convert', {
      method: 'POST',
      headers: { 'x-csrf-token': 'mock-csrf-token' },
      body: JSON.stringify({ role: 'Buyer', notes: 'some notes' }),
    });
  });
});
