// Copyright (c) 2026 Propely. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

import { describe, expect, it, vi, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';

vi.mock('@/lib/api', () => ({
  apiFetch: vi.fn(),
}));

import { apiFetch } from '@/lib/api';
import { useSetPermissionOverride, useRemovePermissionOverride } from './permissions';

const mockApiFetch = vi.mocked(apiFetch);

describe('useSetPermissionOverride', () => {
  beforeEach(() => vi.clearAllMocks());

  it('returns a callback function', () => {
    const { result } = renderHook(() => useSetPermissionOverride('org-1'));
    expect(typeof result.current).toBe('function');
  });

  it('calls apiFetch with PUT method', async () => {
    mockApiFetch.mockResolvedValueOnce(undefined as never);

    const { result } = renderHook(() => useSetPermissionOverride('org-1'));
    await act(async () => {
      await result.current('user-1', 'properties.create', true);
    });

    expect(mockApiFetch).toHaveBeenCalledWith('/orgs/org-1/permissions/user-1/properties.create', {
      method: 'PUT',
      body: JSON.stringify({ granted: true }),
    });
  });
});

describe('useRemovePermissionOverride', () => {
  beforeEach(() => vi.clearAllMocks());

  it('returns a callback function', () => {
    const { result } = renderHook(() => useRemovePermissionOverride('org-1'));
    expect(typeof result.current).toBe('function');
  });

  it('calls apiFetch with DELETE method', async () => {
    mockApiFetch.mockResolvedValueOnce(undefined as never);

    const { result } = renderHook(() => useRemovePermissionOverride('org-1'));
    await act(async () => {
      await result.current('user-1', 'properties.create');
    });

    expect(mockApiFetch).toHaveBeenCalledWith('/orgs/org-1/permissions/user-1/properties.create', {
      method: 'DELETE',
    });
  });
});
